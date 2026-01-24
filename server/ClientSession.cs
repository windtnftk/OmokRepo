using System.Net.Sockets;
using System.Net;

namespace ServerApp
{
    public class ClientSession
    {
        public Socket Socket { get; init; }
        public IPEndPoint EndPoint { get; init; }

        public ClientSession(Socket socket, IPEndPoint endPoint)
        {
            Socket = socket;
            EndPoint = endPoint;
        }
    }
}
