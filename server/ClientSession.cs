using System.Net.Sockets;
using System.Net;

namespace ServerApp
{
    public class ClientSession
    {
        public Socket Socket { get; init; }
        public IPEndPoint EndPoint { get; init; }

        // 역할: 클라이언트 소켓과 엔드포인트를 보관한다.
        public ClientSession(Socket socket, IPEndPoint endPoint)
        {
            Socket = socket;
            EndPoint = endPoint;
        }
    }
}
