using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Protocol;
using Protocol_IO;
using NetworkSend;
using System.Collections.Generic;

namespace ServerApp
{
    public class Server : IDisposable
    {
        private const int Port = 9000;
        private const int MaxClients = 2;
        private const long PingIntervalMs = 5000;
        private const long TimeoutMs = 15000;
        private const long MatchTimeoutMs = 30000;

        private readonly Socket _listenSocket;
        private readonly ConcurrentDictionary<ClientSession, Thread> _pendingSessions = new();
        private readonly ConcurrentQueue<PacketEvent> _inboundQueue = new();
        private readonly ConcurrentQueue<SystemEvent> _systemQueue = new();
        private readonly UserManager _userManager = new();
        private readonly Dictionary<int, Room> _rooms = new();
        private volatile bool _running;
        private Thread _acceptThread;
        private Thread _logicThread;
        private int _clientCount = 0;
        private int _nextRoomId = 1;
        private int? _waitingUserId;
        private long _waitingUserEnqueueTick;
        private long _lastHeartbeatTick;
        private bool _disposed;

        // 역할: 서버 리슨 소켓을 초기화한다.
        public Server()
        {
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        // 역할: 서버 실행 진입점을 제공한다.
        public static void Main(string[] args)
        {
            using var server = new Server();
            server.Start();

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                server.Stop();
            };

            server.Run();
        }

        // 역할: 리슨 및 로직 스레드를 시작한다.
        public void Start()
        {
            _listenSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            _listenSocket.Listen(int.MaxValue);
            _running = true;
            _lastHeartbeatTick = Environment.TickCount64;

            _acceptThread = new Thread(AcceptLoop) { IsBackground = true };
            _logicThread = new Thread(LogicLoop) { IsBackground = true };

            _acceptThread.Start();
            _logicThread.Start();

            Console.WriteLine($"서버 ON : {Port} (멀티 클라)");
        }

        // 역할: 서버 종료 신호를 기다린다.
        public void Run()
        {
            while (_running)
            {
                Thread.Sleep(100);
            }

            Stop();
        }

        // 역할: 서버를 안전하게 정지한다.
        public void Stop()
        {
            if (!_running)
            {
                return;
            }

            _running = false;

            try
            {
                _listenSocket.Close();
            }
            catch
            {
            }

            try
            {
                _listenSocket.Dispose();
            }
            catch
            {
            }

            JoinThread(_acceptThread);
            JoinThread(_logicThread);

            foreach (var kvp in _pendingSessions)
            {
                SafeCloseSession(kvp.Key);
            }

            foreach (var kvp in _pendingSessions)
            {
                JoinThread(kvp.Value);
            }

            _pendingSessions.Clear();
            _userManager.CloseAll();
        }

        // 역할: 리소스를 해제한다.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // 역할: 내부 리소스를 정리한다.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Stop();
            }

            _disposed = true;
        }

        // 역할: 신규 클라이언트 접속을 수락한다.
        private void AcceptLoop()
        {
            while (_running)
            {
                Socket clientSocket;
                try
                {
                    clientSocket = _listenSocket.Accept();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException)
                {
                    if (!_running)
                    {
                        break;
                    }
                    continue;
                }

                var endPoint = clientSocket.RemoteEndPoint as IPEndPoint;
                if (endPoint == null)
                {
                    clientSocket.Close();
                    continue;
                }

                var session = new ClientSession(clientSocket, endPoint);
                var worker = new Thread(() => ClientWorker(session)) { IsBackground = true };
                if (_pendingSessions.TryAdd(session, worker))
                {
                    worker.Start();
                }
                else
                {
                    clientSocket.Close();
                }
            }
        }

        // 역할: 클라이언트 세션의 수신 루프를 처리한다.
        private void ClientWorker(ClientSession session)
        {
            Protocol.IP_Port ipPort = Protocol_IO.ProtocolIO.GetIpPort(session.EndPoint);
            bool handshakeDone = false;
            int userId = 0;

            try
            {
                while (_running)
                {
                    if (!Protocol_IO.ProtocolIO.ReceivePacket(session.Socket, out PacketType packetType, out byte[] payloadBytes))
                    {
                        if (handshakeDone)
                        {
                            Console.WriteLine($"접속 종료: userId={userId}, reason=recv-failed");
                        }
                        else
                        {
                            LogDisconnectOrError(ipPort);
                        }
                        break;
                    }

                    session.LastSeenTick = Environment.TickCount64;

                    if (!handshakeDone)
                    {
                        if (!TryHandshake(packetType, payloadBytes, session, ipPort))
                        {
                            break;
                        }
                        handshakeDone = true;
                        _pendingSessions.TryRemove(session, out Thread workerThread);
                        userId = _userManager.RegisterUser(session, workerThread ?? Thread.CurrentThread);
                        continue;
                    }

                    _inboundQueue.Enqueue(new PacketEvent(userId, packetType, payloadBytes));
                }
            }
            finally
            {
                if (handshakeDone)
                {
                    int newCount = Interlocked.Decrement(ref _clientCount);
                    if (newCount < 0) Interlocked.Exchange(ref _clientCount, 0);
                    Console.WriteLine($"접속 해제: {(string.IsNullOrEmpty(ipPort.ip) ? "unknown" : ipPort.ip)}:{ipPort.port} -> 현재 접속 수: {Math.Max(0, _clientCount)}");
                    _systemQueue.Enqueue(new SystemEvent(SystemEventType.Disconnect, userId));
                }

                _pendingSessions.TryRemove(session, out _);
                SafeCloseSession(session);
            }
        }

        // 역할: 초기 핸드셰이크를 검증하고 환영 메시지를 전송한다.
        private bool TryHandshake(PacketType packetType, byte[] payloadBytes, ClientSession session, Protocol.IP_Port ipPort)
        {
            if (packetType != PacketType.C2S_Hello)
            {
                ServerSend.Error(session.Socket, "handshake required");
                return false;
            }

            string message = PacketSerializer.ParseString(payloadBytes);
            if (message != "Hello")
            {
                ServerSend.Error(session.Socket, "invalid hello payload");
                return false;
            }

            int currentCount = Interlocked.Increment(ref _clientCount);
            if (currentCount > MaxClients)
            {
                Interlocked.Decrement(ref _clientCount);
                string fullMessage = "현재 인원이 가득찼습니다.";
                ServerSend.Error(session.Socket, fullMessage);
                Console.WriteLine($"거부된 연결({(string.IsNullOrEmpty(ipPort.ip) ? "unknown" : ipPort.ip)}:{ipPort.port}) - 서버가 가득 참 (현재: {currentCount})");
                return false;
            }


            string welcomeMessage = $"Welcome. 현재 접속 인원: {currentCount}";
            ServerSend.Welcome(session.Socket, welcomeMessage);

            Console.WriteLine($"핸드셰이크 완료: {(string.IsNullOrEmpty(ipPort.ip) ? "unknown" : ipPort.ip)}:{ipPort.port} -> 현재 접속 수: {currentCount}");
            return true;
        }

        // 역할: 수신된 시스템/패킷 이벤트를 처리한다.
        private void LogicLoop()
        {
            while (_running)
            {
                while (_systemQueue.TryDequeue(out SystemEvent systemEvent))
                {
                    HandleSystemEvent(systemEvent);
                }

                while (_inboundQueue.TryDequeue(out PacketEvent packetEvent))
                {
                    HandlePacketEvent(packetEvent);
                }

                long now = Environment.TickCount64;
                if (now - _lastHeartbeatTick >= PingIntervalMs)
                {
                    UpdateHeartbeat(now);
                    _lastHeartbeatTick = now;
                }

                if (_waitingUserId.HasValue && now - _waitingUserEnqueueTick >= MatchTimeoutMs)
                {
                    HandleMatchTimeout();
                }

                Thread.Sleep(10);
            }

            while (_systemQueue.TryDequeue(out SystemEvent remainingSystem))
            {
                HandleSystemEvent(remainingSystem);
            }

            while (_inboundQueue.TryDequeue(out PacketEvent remaining))
            {
                HandlePacketEvent(remaining);
            }
        }

        // 역할: 사용자 요청 패킷을 분기 처리한다.
        private void HandlePacketEvent(PacketEvent packetEvent)
        {
            if (!_userManager.TryGetSession(packetEvent.UserId, out ClientSession session))
            {
                return;
            }

            Protocol.IP_Port ipPort = Protocol_IO.ProtocolIO.GetIpPort(session.EndPoint);

            switch (packetEvent.Type)
            {
                case PacketType.C2S_ChatMessage:
                    HandleChatMessage(packetEvent.UserId, session.Socket, ipPort, packetEvent.Payload);
                    break;
                case PacketType.C2S_MatchRequest:
                    HandleMatchRequest(packetEvent.UserId);
                    break;
                case PacketType.C2S_Pong:
                case PacketType.C2S_Ping:
                    break;
                case PacketType.C2S_PlaceStoneRequest:
                    HandlePlaceStoneRequest(packetEvent.UserId, session.Socket, ipPort, packetEvent.Payload);
                    break;
                case PacketType.C2S_EndGame:
                    HandleEndGame(packetEvent.UserId);
                    break;
                default:
                    ServerSend.Error(session.Socket, "unknown type");
                    break;
            }
        }

        // 역할: 채팅 메시지를 룸 참여자에게 중계한다.
        private void HandleChatMessage(int userId, Socket clientSocket, Protocol.IP_Port ipPort, byte[] payloadBytes)
        {
            if (!_userManager.TryGetUser(userId, out SessionInfo userInfo))
            {
                return;
            }

            if (userInfo.State == UserState.InRoom && userInfo.RoomId.HasValue && _rooms.TryGetValue(userInfo.RoomId.Value, out Room room))
            {
                string messageText = PacketSerializer.ParseString(payloadBytes);
                Console.WriteLine($"받은 단어({(string.IsNullOrEmpty(ipPort.ip) ? "unknown" : ipPort.ip)}:{ipPort.port}): {messageText}");
                foreach (int playerId in room.GetPlayers())
                {
                    if (_userManager.TryGetSession(playerId, out ClientSession playerSession))
                    {
                        ServerSend.ChatMessage(playerSession.Socket, messageText);
                    }
                }
            }
        }


        // 역할: 착수 요청을 검증하고 룸 참여자에게 전파한다.
        private void HandlePlaceStoneRequest(int userId, Socket clientSocket, Protocol.IP_Port ipPort, byte[] payloadBytes)
        {
            if (!PacketSerializer.TryParsePlace(payloadBytes, out uint x, out uint y))
            {
                ServerSend.Error(clientSocket, "bad position payload");
                return;
            }

            Console.WriteLine($"받은 좌표({(string.IsNullOrEmpty(ipPort.ip) ? "unknown" : ipPort.ip)}:{ipPort.port}): ({x},{y})");
            if (!_userManager.TryGetUser(userId, out SessionInfo userInfo) || !userInfo.RoomId.HasValue)
            {
                ServerSend.Error(clientSocket, "not in room");
                return;
            }

            if (!_rooms.TryGetValue(userInfo.RoomId.Value, out Room room))
            {
                ServerSend.Error(clientSocket, "room not found");
                return;
            }

            if (!room.TryPlace(userId, x, y, out string rejectReason))
            {
                ServerSend.Error(clientSocket, rejectReason ?? "invalid move");
                return;
            }

            foreach (int playerId in room.GetPlayers())
            {
                if (_userManager.TryGetSession(playerId, out ClientSession playerSession))
                {
                    ServerSend.PlaceStoneAck(playerSession.Socket, x, y);
                }
            }
        }

        // 역할: 매칭 요청을 처리하고 룸을 생성한다.
        private void HandleMatchRequest(int userId)
        {
            if (!_userManager.TryGetUser(userId, out SessionInfo userInfo))
            {
                return;
            }

            if (userInfo.State != UserState.Connected)
            {
                return;
            }

            _userManager.SetState(userId, UserState.Matching);

            if (_waitingUserId == null)
            {
                _waitingUserId = userId;
                _waitingUserEnqueueTick = Environment.TickCount64;
                return;
            }

            if (_waitingUserId == userId)
            {
                return;
            }

            int opponentId = _waitingUserId.Value;
            if (!_userManager.TryGetUser(opponentId, out SessionInfo opponentInfo) || opponentInfo.State != UserState.Matching)
            {
                _waitingUserId = userId;
                return;
            }

            int roomId = _nextRoomId++;
            var room = new Room(roomId, opponentId, userId);
            room.Start();
            _rooms[roomId] = room;

            _userManager.SetRoom(opponentId, roomId);
            _userManager.SetRoom(userId, roomId);
            _userManager.SetState(opponentId, UserState.InRoom);
            _userManager.SetState(userId, UserState.InRoom);
            _waitingUserId = null;
            _waitingUserEnqueueTick = 0;

            if (_userManager.TryGetSession(opponentId, out ClientSession opponentSession))
            {
                ServerSend.MatchFound(opponentSession.Socket, roomId, (uint)Stone.Black, true);
            }

            if (_userManager.TryGetSession(userId, out ClientSession userSession))
            {
                ServerSend.MatchFound(userSession.Socket, roomId, (uint)Stone.White, false);
            }
        }

        // 역할: 게임 종료 요청을 처리하고 룸을 정리한다.
        private void HandleEndGame(int userId)
        {
            if (!_userManager.TryGetUser(userId, out SessionInfo userInfo) || !userInfo.RoomId.HasValue)
            {
                return;
            }

            int roomId = userInfo.RoomId.Value;
            if (!_rooms.TryGetValue(roomId, out Room room))
            {
                _userManager.SetRoom(userId, null);
                _userManager.SetState(userId, UserState.Connected);
                return;
            }

            foreach (int playerId in room.GetPlayers())
            {
                _userManager.SetRoom(playerId, null);
                _userManager.SetState(playerId, UserState.Connected);
            }

            _rooms.Remove(roomId);
        }

        // 역할: 시스템 이벤트(예: 연결 종료)를 처리한다.
        private void HandleSystemEvent(SystemEvent systemEvent)
        {
            switch (systemEvent.Type)
            {
                case SystemEventType.Disconnect:
                    if (_waitingUserId == systemEvent.UserId)
                    {
                        _waitingUserId = null;
                        _waitingUserEnqueueTick = 0;
                    }
                    _userManager.OnDisconnect(systemEvent.UserId);
                    break;
            }
        }

        private void HandleMatchTimeout()
        {
            int userId = _waitingUserId ?? 0;
            _waitingUserId = null;
            _waitingUserEnqueueTick = 0;

            if (userId == 0)
            {
                return;
            }

            if (_userManager.TryGetSession(userId, out ClientSession session) && session != null && !session.IsClosed)
            {
                ServerSend.MatchFail(session.Socket);
                _userManager.SetState(userId, UserState.Connected);
            }
        }

        private void UpdateHeartbeat(long now)
        {
            List<SessionInfo> sessions = _userManager.GetSessionSnapshot();
            foreach (var info in sessions)
            {
                ClientSession session = info.Session;
                if (session == null || session.IsClosed)
                {
                    continue;
                }

                if (now - session.LastSeenTick >= TimeoutMs)
                {
                    Console.WriteLine($"세션 타임아웃 종료: userId={info.UserId}, reason=timeout");
                    if (_waitingUserId == info.UserId)
                    {
                        _waitingUserId = null;
                        _waitingUserEnqueueTick = 0;
                    }
                    SafeCloseSession(session);
                    continue;
                }

                if (now - session.LastPingSentTick >= PingIntervalMs)
                {
                    if (ServerSend.Ping(session.Socket))
                    {
                        session.LastPingSentTick = now;
                    }
                    else
                    {
                        Console.WriteLine($"세션 Ping 전송 실패: userId={info.UserId}, reason=send-failed");
                        SafeCloseSession(session);
                    }
                }
            }
        }

        // 역할: 접속 종료 로그를 출력한다.
        private static void LogDisconnectOrError(Protocol.IP_Port ipPort)
        {
            Console.WriteLine($"클라이언트 연결 종료: {(string.IsNullOrEmpty(ipPort.ip) ? "unknown" : ipPort.ip)}:{ipPort.port}");
        }

        // 역할: 스레드를 제한 시간 내에 종료 대기한다.
        private static void JoinThread(Thread thread)
        {
            if (thread == null)
            {
                return;
            }

            if (thread.IsAlive)
            {
                thread.Join(500);
            }
        }

        // 역할: 세션 소켓을 안전하게 종료한다.
        private static void SafeCloseSession(ClientSession session)
        {
            if (session?.Socket == null)
            {
                return;
            }

            if (!session.TryMarkClosed())
            {
                return;
            }

            try
            {
                session.Socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }

            try
            {
                session.Socket.Close();
            }
            catch
            {
            }

            try
            {
                session.Socket.Dispose();
            }
            catch
            {
            }
        }

        private record PacketEvent(int UserId, PacketType Type, byte[] Payload);
        private enum SystemEventType
        {
            Disconnect
        }

        private record SystemEvent(SystemEventType Type, int UserId);

    }
}
