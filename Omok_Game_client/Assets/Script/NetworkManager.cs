using NetworkClientApp;
using Protocol;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    private NetworkClient _client; // 니가 만든 클래스

    void Awake()
    {
        // 중복 생성 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _client = new NetworkClient();
    }

    // : ä û Ѵ.
    public void RequestWord(string msg)
    {
        _client.SendWord(msg);
    }

    // :  ǥ û Ѵ.
    public void RequestPosition(uint x, uint y)
    {
        _client.SendPosition(x, y);
    }

    void Update()
    {
        ProcessRecvQueue();
    }

    private void ProcessRecvQueue()
    {
        while (_client.TryDequeue(out PacketEvent packet))
        {
            HandlePacket(packet);
        }
    }

    private void HandlePacket(PacketEvent packet)
    {
        switch (packet.Type)
        {
            case PacketType.S2C_Welcome:
                {
                    ConnectPanel connectPanel = FindObjectOfType<ConnectPanel>();
                    if (connectPanel != null)
                    {
                        connectPanel.HandleConnectSuccess();
                    }
                    break;
                }
            case PacketType.S2C_MatchFound:
                {
                    MatchPanel matchPanel = FindObjectOfType<MatchPanel>();
                    if (matchPanel != null)
                    {
                        matchPanel.HandleMatchSuccess();
                    }
                    break;
                }
            case PacketType.S2C_MatchFail:
                {
                    MatchPanel matchPanel = FindObjectOfType<MatchPanel>();
                    if (matchPanel != null)
                    {
                        matchPanel.HandleMatchFail();
                    }
                    break;
                }
        }

    }
    public bool Connect(string ip, int port)
    {
        return _client.Connect(ip, port);
    }

    // 역할: 서버 연결을 종료한다.
    public void Disconnect()
    {
        _client.Disconnect();
    }

    // 역할: 매칭 요청을 보낸다(예시).
    public void RequestMatch()
    {
        _client.SendMatchRequest();
    }
}
