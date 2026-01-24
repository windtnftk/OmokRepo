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
        private bool _disposed;

        public Server()
        {
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

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

        public void Start()
        {
            _listenSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            _listenSocket.Listen(int.MaxValue);
            _running = true;

            _acceptThread = new Thread(AcceptLoop) { IsBackground = true };
            _logicThread = new Thread(LogicLoop) { IsBackground = true };

            _acceptThread.Start();
            _logicThread.Start();

            Console.WriteLine($"서버 ON : {Port} (멀티 클라)");
        }

        public void Run()
        {
            while (_running)
            {
                Thread.Sleep(100);
            }

            Stop();
        }

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

        private void ClientWorker(ClientSession session)
        {
            Protocol.IP_Port ipPort = Protocol_IO.ProtocolIO.GetIpPort(session.EndPoint);
            bool handshakeDone = false;
            int userId = 0;

            try
            {
                while (_running)
                {
                    if (!Protocol_IO.ProtocolIO.ReceivePacket(session.Socket, out PacketType ptype, out byte[] payload))
                    {
                        LogDisconnectOrError(ipPort);
                        break;
                    }

                    if (!handshakeDone)
                    {
                        if (!TryHandshake(ptype, payload, session, ipPort))
                        {
                            break;
                        }
                        handshakeDone = true;
                        _pendingSessions.TryRemove(session, out Thread workerThread);
                        userId = _userManager.RegisterUser(session, workerThread ?? Thread.CurrentThread);
                        continue;
                    }

                    _inboundQueue.Enqueue(new PacketEvent(userId, ptype, payload));
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

        private bool TryHandshake(PacketType ptype, byte[] payload, ClientSession session, Protocol.IP_Port ipPort)
        {
            if (ptype != PacketType.C2S_Hello)
            {
                ServerSend.Error(session.Socket, "handshake required");
                return false;
            }

            string msg = PacketSerializer.ParseString(payload);
            if (msg != "Hello")
            {
                ServerSend.Error(session.Socket, "invalid hello payload");
                return false;
            }

            int cur = Interlocked.Increment(ref _clientCount);
            if (cur > MaxClients)
            {
                Interlocked.Decrement(ref _clientCount);
                string fullMsg = "현재 인원이 가득찼습니다.";
                ServerSend.Error(session.Socket, fullMsg);
                Console.WriteLine($"거부된 연결({(string.IsNullOrEmpty(ipPort.ip) ? "unknown" : ipPort.ip)}:{ipPort.port}) - 서버가 가득 참 (현재: {cur})");
                return false;
            }


            string welcome = $"Welcome. 현재 접속 인원: {cur}";
            ServerSend.Welcome(session.Socket, welcome);

            Console.WriteLine($"핸드셰이크 완료: {(string.IsNullOrEmpty(ipPort.ip) ? "unknown" : ipPort.ip)}:{ipPort.port} -> 현재 접속 수: {cur}");
            return true;
        }

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
                case PacketType.C2S_PlaceStoneRequest:
                    HandlePosition(packetEvent.UserId, session.Socket, ipPort, packetEvent.Payload);
                    break;
                case PacketType.C2S_EndGame:
                    HandleEndGame(packetEvent.UserId);
                    break;
                default:
                    ServerSend.Error(session.Socket, "unknown type");
                    break;
            }
        }

        private void HandleChatMessage(int userId, Socket clientSock, Protocol.IP_Port ipPort, byte[] payload)
        {
            if (!_userManager.TryGetUser(userId, out SessionInfo info))
            {
                return;
            }

            if (info.State == UserState.InRoom && info.RoomId.HasValue && _rooms.TryGetValue(info.RoomId.Value, out Room room))
            {
                string text = PacketSerializer.ParseString(payload);
                Console.WriteLine($"받은 단어({(string.IsNullOrEmpty(ipPort.ip) ? "unknown" : ipPort.ip)}:{ipPort.port}): {text}");
                foreach (int playerId in room.GetPlayers())
                {
                    if (_userManager.TryGetSession(playerId, out ClientSession playerSession))
                    {
                        ServerSend.ChatMessage(playerSession.Socket, text);
                    }
                }
            }
        }


        private void HandlePosition(int userId, Socket clientSock, Protocol.IP_Port ipPort, byte[] payload)
        {
            if (!PacketSerializer.TryParsePlace(payload, out uint x, out uint y))
            {
                ServerSend.Error(clientSock, "bad position payload");
                return;
            }

            Console.WriteLine($"받은 좌표({(string.IsNullOrEmpty(ipPort.ip) ? "unknown" : ipPort.ip)}:{ipPort.port}): ({x},{y})");
            if (!_userManager.TryGetUser(userId, out SessionInfo info) || !info.RoomId.HasValue)
            {
                ServerSend.Error(clientSock, "not in room");
                return;
            }

            if (!_rooms.TryGetValue(info.RoomId.Value, out Room room))
            {
                ServerSend.Error(clientSock, "room not found");
                return;
            }

            if (!room.TryPlace(userId, x, y, out string rejectReason))
            {
                ServerSend.Error(clientSock, rejectReason ?? "invalid move");
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

        private void HandleMatchRequest(int userId)
        {
            if (!_userManager.TryGetUser(userId, out SessionInfo info))
            {
                return;
            }

            if (info.State != UserState.Connected)
            {
                return;
            }

            _userManager.SetState(userId, UserState.Matching);

            if (_waitingUserId == null)
            {
                _waitingUserId = userId;
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

            if (_userManager.TryGetSession(opponentId, out ClientSession opponentSession))
            {
                ServerSend.MatchFound(opponentSession.Socket, roomId, (uint)Stone.Black, true);
            }

            if (_userManager.TryGetSession(userId, out ClientSession userSession))
            {
                ServerSend.MatchFound(userSession.Socket, roomId, (uint)Stone.White, false);
            }
        }

        private void HandleEndGame(int userId)
        {
            if (!_userManager.TryGetUser(userId, out SessionInfo info) || !info.RoomId.HasValue)
            {
                return;
            }

            int roomId = info.RoomId.Value;
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

        private void HandleSystemEvent(SystemEvent systemEvent)
        {
            switch (systemEvent.Type)
            {
                case SystemEventType.Disconnect:
                    if (_waitingUserId == systemEvent.UserId)
                    {
                        _waitingUserId = null;
                    }
                    _userManager.OnDisconnect(systemEvent.UserId);
                    break;
            }
        }

        private static void LogDisconnectOrError(Protocol.IP_Port ipPort)
        {
            Console.WriteLine($"클라이언트 연결 종료: {(string.IsNullOrEmpty(ipPort.ip) ? "unknown" : ipPort.ip)}:{ipPort.port}");
        }

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

        private static void SafeCloseSession(ClientSession session)
        {
            if (session?.Socket == null)
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
