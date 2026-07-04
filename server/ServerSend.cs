using System.Net.Sockets;
using Protocol;
using Protocol_IO;

namespace NetworkSend
{
    public static class ServerSend
    {
        // 역할: 환영 메시지 패킷을 전송한다.
        public static bool Welcome(Socket socket, string text)
        {
            byte[] payloadBytes = PacketSerializer.BuildWelcome(text);
            return Protocol_IO.ProtocolIO.SendPacket(socket, PacketType.S2C_Welcome, payloadBytes, (uint)payloadBytes.Length);
        }

        // 역할: 채팅 메시지 패킷을 전송한다.
        public static bool ChatMessage(Socket socket, string text)
        {
            byte[] payloadBytes = PacketSerializer.BuildChatMessage(text);
            return Protocol_IO.ProtocolIO.SendPacket(socket, PacketType.S2C_ChatMessage, payloadBytes, (uint)payloadBytes.Length);
        }

        // 역할: 착수 승인 패킷을 전송한다.
        public static bool PlaceStoneAck(Socket socket, uint x, uint y, uint stone)
        {
            byte[] payloadBytes = PacketSerializer.BuildPlaceStoneAck(x, y, stone);
            return Protocol_IO.ProtocolIO.SendPacket(socket, PacketType.S2C_PlaceStoneAck, payloadBytes, (uint)payloadBytes.Length);
        }

        // 역할: 오류 메시지 패킷을 전송한다.
        public static bool Error(Socket socket, string text)
        {
            byte[] payloadBytes = PacketSerializer.BuildError(text);
            return Protocol_IO.ProtocolIO.SendPacket(socket, PacketType.S2C_Error, payloadBytes, (uint)payloadBytes.Length);
        }

        // 역할: 매칭 완료 패킷을 전송한다.
        public static bool MatchFound(Socket socket, int roomId, uint myColor, bool isMyTurn)
        {
            uint turnFlag = isMyTurn ? 1u : 0u;
            byte[] payloadBytes = PacketSerializer.MakeMatchFound(roomId, myColor, turnFlag);
            return Protocol_IO.ProtocolIO.SendPacket(socket, PacketType.S2C_MatchFound, payloadBytes, (uint)payloadBytes.Length);
        }

        public static bool MatchFail(Socket socket)
        {
            byte[] payloadBytes = PacketSerializer.BuildEmpty();
            return Protocol_IO.ProtocolIO.SendPacket(socket, PacketType.S2C_MatchFail, payloadBytes, (uint)payloadBytes.Length);
        }

        public static bool Ping(Socket socket)
        {
            byte[] payloadBytes = PacketSerializer.BuildEmpty();
            return Protocol_IO.ProtocolIO.SendPacket(socket, PacketType.S2C_Ping, payloadBytes, (uint)payloadBytes.Length);
        }
    }
}
