using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace ServerApp
{
    public class ClientSession
    {
        public Socket Socket { get; init; }
        public IPEndPoint EndPoint { get; init; }
        public long LastSeenTick { get; set; }
        public long LastPingSentTick { get; set; }
        private int _closed;
        public bool IsClosed => _closed != 0;

        // 역할: 클라이언트 소켓과 엔드포인트를 보관한다.
        public ClientSession(Socket socket, IPEndPoint endPoint)
        {
            Socket = socket;
            EndPoint = endPoint;
            long now = System.Environment.TickCount64;
            LastSeenTick = now;
            LastPingSentTick = now;
        }

        public bool TryMarkClosed()
        {
            return Interlocked.Exchange(ref _closed, 1) == 0;
        }
    }
}
