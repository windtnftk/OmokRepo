using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Assets.Script;
using Protocol;

namespace NetworkClientApp
{
    public class MockNetworkClient : INetworkClient
    {
        private readonly ConcurrentQueue<PacketEvent> _recvQueue = new();
        private readonly BoardLogic _boardLogic = new();
        private bool _connected;
        private bool _isBlackTurn = true;

        public bool Connect(string ip, int port)
        {
            _connected = true;
            EnqueueDelayed(new PacketEvent(PacketType.S2C_Welcome, Array.Empty<byte>()));
            return true;
        }

        public void SendMatchRequest()
        {
            if (!_connected)
            {
                return;
            }

            EnqueueDelayed(new PacketEvent(PacketType.S2C_MatchFound, PacketSerializer.MakeMatchFound(1, 1u, 1u)));
        }

        public bool SendPosition(uint x, uint y)
        {
            if (!_connected)
            {
                return false;
            }

            bool success = false;
            uint stone = _isBlackTurn ? 1u : 2u;

            if (x <= int.MaxValue && y <= int.MaxValue)
            {
                PlaceSucces result = _boardLogic.PlaceStone((int)x, (int)y, _isBlackTurn ? Stone.Black : Stone.White);
                if (result != PlaceSucces.None)
                {
                    success = true;
                    _isBlackTurn = !_isBlackTurn;
                }
            }

            EnqueueDelayed(new PacketEvent(PacketType.S2C_PlaceStoneAck, Array.Empty<byte>(), success, x, y, stone));
            return true;
        }

        public bool TryDequeue(out PacketEvent packet)
        {
            return _recvQueue.TryDequeue(out packet);
        }

        private async void EnqueueDelayed(PacketEvent packet)
        {
            await Task.Delay(200);
            _recvQueue.Enqueue(packet);
        }
    }
}
