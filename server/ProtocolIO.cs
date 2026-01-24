using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using Protocol;

namespace Protocol_IO
{
    public static class ProtocolIO
    {
        public static bool SendAll(Socket socket, byte[] data, int len)
        {
            try
            {
                int total = 0;
                while (total < len)
                {
                    int sent = socket.Send(data, total, len - total, SocketFlags.None);
                    if (sent <= 0) return false;
                    total += sent;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool ReceiveExact(Socket socket, byte[] buffer, int len)
        {
            try
            {
                int total = 0;
                while (total < len)
                {
                    int received = socket.Receive(buffer, total, len - total, SocketFlags.None);
                    if (received == 0) return false;   // 연결 종료
                    if (received < 0) return false;    // 오류
                    total += received;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool SendPacket(Socket socket, PacketType type, byte[] payload, uint len)
        {
            try
            {
                if (len > ProtocolHelper.MAX_PAYLOAD)
                {
                    Console.Error.WriteLine($"SendPacket: payload too large: {len}");
                    return false;
                }

                byte[] header = new byte[8];
                BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(0, 4), (uint)type);
                BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4, 4), len);

                if (!SendAll(socket, header, header.Length))
                {
                    Console.Error.WriteLine("SendPacket: 헤더 전송 실패");
                    return false;
                }

                if (len == 0) return true;

                if (payload == null || payload.Length < len)
                {
                    Console.Error.WriteLine("SendPacket: len>0 but payload is null or too small");
                    return false;
                }

                if (!SendAll(socket, payload, (int)len))
                {
                    Console.Error.WriteLine("SendPacket: payload 전송 실패");
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool ReceivePacket(Socket socket, out PacketType outType, out byte[] outPayload)
        {
            outType = PacketType.S2C_Error;
            outPayload = Array.Empty<byte>();

            try
            {
                byte[] header = new byte[8];
                if (!ReceiveExact(socket, header, header.Length))
                {
                    Console.Error.WriteLine("RecvPacket: 헤더 수신 실패");
                    return false;
                }

                uint type = BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(0, 4));
                uint len = BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(4, 4));

                outType = (PacketType)type;

                if (len > ProtocolHelper.MAX_PAYLOAD)
                {
                    Console.Error.WriteLine($"RecvPacket: payload too large: {len}");
                    return false;
                }

                if (len > 0)
                {
                    outPayload = new byte[len];
                    if (!ReceiveExact(socket, outPayload, (int)len))
                    {
                        Console.Error.WriteLine("RecvPacket: payload 수신 실패");
                        return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetIpString(IPEndPoint endPoint)
        {
            return endPoint.Address.ToString();
        }

        public static int GetPort(IPEndPoint endPoint)
        {
            return endPoint.Port;
        }

        public static IP_Port GetIpPort(IPEndPoint endPoint)
        {
            return new IP_Port
            {
                ip = GetIpString(endPoint),
                port = GetPort(endPoint)
            };
        }
    }
}
