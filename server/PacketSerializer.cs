using System;
using System.Buffers.Binary;
using System.Text;

namespace Protocol
{
    public static class PacketSerializer
    {
        public const uint PositionPayloadSize = 8u;
        private const int MatchFoundPayloadSize = 12;

        public static byte[] BuildHello()
        {
            return BuildString("Hello");
        }

        public static byte[] BuildChat(string text)
        {
            return BuildString(text ?? string.Empty);
        }

        public static byte[] BuildChatMessage(string text)
        {
            return BuildChat(text);
        }

        public static byte[] BuildWelcome(string text)
        {
            return BuildString(text ?? string.Empty);
        }

        public static byte[] BuildError(string text)
        {
            return BuildString(text ?? string.Empty);
        }

        public static byte[] BuildPlace(uint x, uint y)
        {
            var buffer = new byte[PositionPayloadSize];
            BinaryPrimitives.WriteUInt32BigEndian(buffer.AsSpan(0, 4), x);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.AsSpan(4, 4), y);
            return buffer;
        }

        public static byte[] BuildEmpty()
        {
            return Array.Empty<byte>();
        }

        public static byte[] MakeMatchFound(int roomId, uint myColor, uint isMyTurn)
        {
            var buffer = new byte[MatchFoundPayloadSize];
            BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(0, 4), roomId);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.AsSpan(4, 4), myColor);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.AsSpan(8, 4), isMyTurn);
            return buffer;
        }

        // 클라 전용
        public static bool TryParseMatchFound(ReadOnlySpan<byte> payload, out int roomId, out uint myColor, out uint isMyTurn)
        {
            roomId = 0;
            myColor = 0;
            isMyTurn = 0;

            if (payload.Length != MatchFoundPayloadSize)
            {
                return false;
            }

            roomId = BinaryPrimitives.ReadInt32BigEndian(payload.Slice(0, 4));
            myColor = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(4, 4));
            isMyTurn = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(8, 4));
            return true;
        }

        public static string ParseString(ReadOnlySpan<byte> payload)
        {
            return Encoding.UTF8.GetString(payload);
        }

        public static bool TryParsePlace(ReadOnlySpan<byte> payload, out uint outX, out uint outY)
        {
            outX = 0;
            outY = 0;

            if (payload.Length != PositionPayloadSize)
            {
                return false;
            }

            outX = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(0, 4));
            outY = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(4, 4));
            return true;
        }

        private static byte[] BuildString(string text)
        {
            return Encoding.UTF8.GetBytes(text ?? string.Empty);
        }
    }
}
