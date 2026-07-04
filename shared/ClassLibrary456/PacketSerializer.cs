using System;
using System.Buffers.Binary;
using System.Text;

namespace Protocol
{
    public static class PacketSerializer
    {
        public const uint PositionPayloadSize = 8u;
        public const uint PlaceStoneAckPayloadSize = 12u;
        private const int MatchFoundPayloadSize = 12;

        // 역할: 핸드셰이크용 문자열 payload를 생성한다.
        public static byte[] BuildHello()
        {
            return BuildString("Hello");
        }

        // 역할: 채팅 메시지 payload를 생성한다.
        public static byte[] BuildChat(string text)
        {
            return BuildString(text ?? string.Empty);
        }

        // 역할: 채팅 메시지 payload를 생성한다.
        public static byte[] BuildChatMessage(string text)
        {
            return BuildChat(text);
        }

        // 역할: 환영 메시지 payload를 생성한다.
        public static byte[] BuildWelcome(string text)
        {
            return BuildString(text ?? string.Empty);
        }

        // 역할: 오류 메시지 payload를 생성한다.
        public static byte[] BuildError(string text)
        {
            return BuildString(text ?? string.Empty);
        }

        // 역할: 착수 좌표 payload를 생성한다.
        public static byte[] BuildPlace(uint x, uint y)
        {
            var payloadBuffer = new byte[PositionPayloadSize];
            BinaryPrimitives.WriteUInt32BigEndian(payloadBuffer.AsSpan(0, 4), x);
            BinaryPrimitives.WriteUInt32BigEndian(payloadBuffer.AsSpan(4, 4), y);
            return payloadBuffer;
        }

        // 역할: 착수 승인 payload를 생성한다.
        public static byte[] BuildPlaceStoneAck(uint x, uint y, uint stone)
        {
            var payloadBuffer = new byte[PlaceStoneAckPayloadSize];
            BinaryPrimitives.WriteUInt32BigEndian(payloadBuffer.AsSpan(0, 4), x);
            BinaryPrimitives.WriteUInt32BigEndian(payloadBuffer.AsSpan(4, 4), y);
            BinaryPrimitives.WriteUInt32BigEndian(payloadBuffer.AsSpan(8, 4), stone);
            return payloadBuffer;
        }

        // 역할: 빈 payload를 반환한다.
        public static byte[] BuildEmpty()
        {
            return Array.Empty<byte>();
        }

        // 역할: 매칭 완료 payload를 생성한다.
        public static byte[] MakeMatchFound(int roomId, uint myColor, uint isMyTurn)
        {
            var payloadBuffer = new byte[MatchFoundPayloadSize];
            BinaryPrimitives.WriteInt32BigEndian(payloadBuffer.AsSpan(0, 4), roomId);
            BinaryPrimitives.WriteUInt32BigEndian(payloadBuffer.AsSpan(4, 4), myColor);
            BinaryPrimitives.WriteUInt32BigEndian(payloadBuffer.AsSpan(8, 4), isMyTurn);
            return payloadBuffer;
        }

        // 클라 전용
        // 역할: 매칭 완료 payload를 파싱한다.
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

        // 역할: 문자열 payload를 디코딩한다.
        public static string ParseString(ReadOnlySpan<byte> payload)
        {
            return Encoding.UTF8.GetString(payload);
        }

        // 역할: 착수 좌표 payload를 파싱한다.
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

        // 역할: 착수 승인 payload를 파싱한다.
        public static bool TryParsePlaceStoneAck(ReadOnlySpan<byte> payload, out uint outX, out uint outY, out uint stone)
        {
            outX = 0;
            outY = 0;
            stone = 0;

            if (payload.Length != PlaceStoneAckPayloadSize)
            {
                return false;
            }

            outX = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(0, 4));
            outY = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(4, 4));
            stone = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(8, 4));
            return true;
        }

        // 역할: 문자열을 UTF-8 bytes로 변환한다.
        private static byte[] BuildString(string text)
        {
            return Encoding.UTF8.GetBytes(text ?? string.Empty);
        }
    }
}
