using Protocol;
using System;
using System.Net.Sockets;

namespace NetworkClientApp
{
    public class PacketEvent
    {
        public PacketType Type { get; }
        public byte[] Payload { get; }

        public PacketEvent(PacketType type, byte[] payload)
        {
            Type = type;
            Payload = payload ?? Array.Empty<byte>();
        }
    }
}