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

        public bool SendWord(string msg)
        {
            if (!_connected || _socket == null) return false;

            lock (_sendLock)
            {
                return ClientSend.Chat(_socket, msg);
            }
        }

        public bool SendPosition(uint x, uint y)
        {
            if (!_connected || _socket == null) return false;

            lock (_sendLock)
            {
                return ClientSend.Place(_socket, x, y);
            }
        }

        public bool TryDequeue(out PacketEvent packet)
        {
            return _recvQueue.TryDequeue(out packet);
        }

        public void Dispose()
        {
            Disconnect();
        }

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
