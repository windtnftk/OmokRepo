using System.Net.Sockets;
using Protocol;
using Protocol_IO;

namespace NetworkSend
{
    public static class ServerSend
    {
        public static bool Welcome(Socket socket, string text)
        {
            byte[] payload = PacketSerializer.BuildWelcome(text);
            return Protocol_IO.ProtocolIO.SendPacket(socket, PacketType.S2C_Welcome, payload, (uint)payload.Length);
        }

        public static bool ChatMessage(Socket socket, string text)
        {
            byte[] payload = PacketSerializer.BuildChatMessage(text);
            return Protocol_IO.ProtocolIO.SendPacket(socket, PacketType.S2C_ChatMessage, payload, (uint)payload.Length);
        }

        public static bool PlaceStoneAck(Socket socket, uint x, uint y)
        {
            byte[] payload = PacketSerializer.BuildPlace(x, y);
            return Protocol_IO.ProtocolIO.SendPacket(socket, PacketType.S2C_PlaceStoneAck, payload, (uint)payload.Length);
        }

        public static bool Error(Socket socket, string text)
        {
            byte[] payload = PacketSerializer.BuildError(text);
            return Protocol_IO.ProtocolIO.SendPacket(socket, PacketType.S2C_Error, payload, (uint)payload.Length);
        }

        public static bool MatchFound(Socket socket, int roomId, uint myColor, bool isMyTurn)
        {
            uint turnFlag = isMyTurn ? 1u : 0u;
            byte[] payload = PacketSerializer.MakeMatchFound(roomId, myColor, turnFlag);
            return Protocol_IO.ProtocolIO.SendPacket(socket, PacketType.S2C_MatchFound, payload, (uint)payload.Length);
        }
    }
}
