using NetworkClientApp;

public interface INetworkClient
{
    bool Connect(string ip, int port);
    void SendMatchRequest();
    void SendPosition(uint x, uint y);
    bool TryDequeue(out PacketEvent packet);
}
