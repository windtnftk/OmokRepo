using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using Protocol;
using Protocol_IO;
using NetworkSend;

namespace NetworkClientApp
{
    public class NetworkClient : IDisposable
    {
        private readonly ConcurrentQueue<PacketEvent> _recvQueue = new();
        private readonly object _sendLock = new();

        private Socket _socket;
        private Thread _recvThread;
        private volatile bool _running;
        private volatile bool _connected;

        public bool IsConnected => _connected;

        // 역할: 서버에 연결하고 핸드셰이크를 수행한다.
        public bool Connect(string host, int port)
        {
            Disconnect();

            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(host, port);

                // Hello -> Welcome 핸드셰이크
                if (!ClientSend.Hello(_socket))
                {
                    CleanupSocket();
                    return false;
                }

                if (!Protocol_IO.ProtocolIO.ReceivePacket(_socket, out PacketType rtype, out byte[] rpayload))
                {
                    CleanupSocket();
                    return false;
                }

                if (rtype != PacketType.S2C_Welcome)
                {
                    CleanupSocket();
                    return false;
                }

                _recvQueue.Enqueue(new PacketEvent(rtype, rpayload));

                _running = true;
                _connected = true;

                _recvThread = new Thread(ReceiveLoop)
                {
                    IsBackground = true,
                    Name = "NetworkClient-Recv"
                };
                _recvThread.Start();

                return true;
            }
            catch
            {
                CleanupSocket();
                return false;
            }
        }

        // 역할: 연결을 종료하고 리소스를 정리한다.
        public void Disconnect()
        {
            _running = false;
            _connected = false;

            try
            {
                _socket?.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                // ignore
            }

            CleanupSocket();

            if (_recvThread != null && _recvThread.IsAlive)
            {
                _recvThread.Join(500);
            }
        }

        // 역할: 채팅 메시지를 서버로 전송한다.
        public bool SendWord(string msg)
        {
            if (!_connected || _socket == null) return false;

            lock (_sendLock)
            {
                // send/recv 스레드에서 소켓 동시 접근 방지
                return ClientSend.Chat(_socket, msg);
            }
        }

        // 역할: 매칭 요청을 서버로 전송한다.
        public bool SendMatchRequest()
        {
            if (!_connected || _socket == null) return false;

            lock (_sendLock)
            {
                // send/recv 스레드에서 소켓 동시 접근 방지
                return ClientSend.MatchRequest(_socket);
            }
        }

        // 역할: 착수 좌표를 서버로 전송한다.
        public bool SendPosition(uint x, uint y)
        {
            if (!_connected || _socket == null) return false;

            lock (_sendLock)
            {
                // send/recv 스레드에서 소켓 동시 접근 방지
                return ClientSend.Place(_socket, x, y);
            }
        }

        // 역할: 수신 큐에서 패킷을 꺼낸다.
        public bool TryDequeue(out PacketEvent packet)
        {
            return _recvQueue.TryDequeue(out packet);
        }

        // 역할: 연결 해제와 리소스 해제를 수행한다.
        public void Dispose()
        {
            Disconnect();
        }

        // 역할: 수신 루프에서 서버 패킷을 큐에 적재한다.
        private void ReceiveLoop()
        {
            while (_running)
            {
                try
                {
                    if (!Protocol_IO.ProtocolIO.ReceivePacket(_socket, out PacketType type, out byte[] payload))
                    {
                        break;
                    }

                    if (type == PacketType.S2C_Ping)
                    {
                        lock (_sendLock)
                        {
                            ClientSend.Pong(_socket);
                        }
                        continue;
                    }

                    _recvQueue.Enqueue(new PacketEvent(type, payload));
                }
                catch (SocketException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch
                {
                    break;
                }
            }

            _connected = false;
            _running = false;
            CleanupSocket();
        }

        // 역할: 소켓을 안전하게 닫고 참조를 제거한다.
        private void CleanupSocket()
        {
            try
            {
                _socket?.Close();
            }
            catch
            {
                // ignore
            }

            _socket = null;
        }
    }
}
