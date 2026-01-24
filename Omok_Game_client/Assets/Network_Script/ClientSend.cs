using System.Net.Sockets;
using Protocol;
using Protocol_IO;

namespace NetworkSend
{
    public static class ClientSend
    {
        // 역할: 핸드셰이크용 Hello 패킷을 전송한다.
        public static bool Hello(Socket socket)
        {
            byte[] payload = PacketSerializer.BuildHello();
            return Protocol_IO.ProtocolIO.SendPacket(socket, PacketType.C2S_Hello, payload, (uint)payload.Length);
        }

        // 역할: 채팅 메시지 패킷을 전송한다.
        public static bool Chat(Socket socket, string text)
        {
            byte[] payload = PacketSerializer.BuildChat(text);
            return Protocol_IO.ProtocolIO.SendPacket(socket, PacketType.C2S_ChatMessage, payload, (uint)payload.Length);
        }

        // 역할: 착수 요청 패킷을 전송한다.
        public static bool Place(Socket socket, uint x, uint y)
        {
            byte[] payload = PacketSerializer.BuildPlace(x, y);
            return Protocol_IO.ProtocolIO.SendPacket(socket, PacketType.C2S_PlaceStoneRequest, payload, (uint)payload.Length);
        }

        // 역할: 매칭 요청 패킷을 전송한다.
        public static bool MatchRequest(Socket socket)
        {
            byte[] payload = PacketSerializer.BuildEmpty();
            return Protocol_IO.ProtocolIO.SendPacket(socket, PacketType.C2S_MatchRequest, payload, (uint)payload.Length);
        }

        // 역할: 게임 종료 알림 패킷을 전송한다.
        public static bool EndGame(Socket socket)
        {
            byte[] payload = PacketSerializer.BuildEmpty();
            return Protocol_IO.ProtocolIO.SendPacket(socket, PacketType.C2S_EndGame, payload, (uint)payload.Length);
        }
    }
}
