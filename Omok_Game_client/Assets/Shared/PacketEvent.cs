using Protocol;
using System;
using System.Net.Sockets;

namespace NetworkClientApp
{
    public class PacketEvent
    {
        public PacketType Type { get; }
        public byte[] Payload { get; }
        public bool HasPlaceStoneAckData { get; }
        public bool PlaceStoneSuccess { get; }
        public uint PlaceX { get; }
        public uint PlaceY { get; }
        public uint PlaceStone { get; }

        // 역할: 패킷 이벤트 객체를 초기화한다.
        public PacketEvent(PacketType type, byte[] payload)
        {
            Type = type;
            Payload = payload ?? Array.Empty<byte>();
        }

        public PacketEvent(PacketType type, byte[] payload, bool placeStoneSuccess, uint placeX, uint placeY, uint placeStone)
        {
            Type = type;
            Payload = payload ?? Array.Empty<byte>();
            HasPlaceStoneAckData = true;
            PlaceStoneSuccess = placeStoneSuccess;
            PlaceX = placeX;
            PlaceY = placeY;
            PlaceStone = placeStone;
        }
    }
}
