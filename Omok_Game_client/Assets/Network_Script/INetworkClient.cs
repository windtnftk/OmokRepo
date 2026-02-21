using NetworkClientApp;

public interface INetworkClient
{
    bool Connect(string ip, int port);
    void SendMatchRequest();
    bool SendPosition(uint x, uint y);
    bool TryDequeue(out PacketEvent packet);
}
