using Protocol;
using System;
using System.Net.Sockets;

namespace NetworkClientApp
{
    public class PacketEvent
    {
        public PacketType Type { get; }
        public byte[] Payload { get; }

        // 역할: 패킷 이벤트 객체를 초기화한다.
        public PacketEvent(PacketType type, byte[] payload)
        {
            Type = type;
            Payload = payload ?? Array.Empty<byte>();
        }
    }
}
