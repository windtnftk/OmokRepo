using NetworkClientApp;
using Protocol;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }
    public bool MockUse = true;
    [SerializeField] private MainBoard mainBoard;

    private INetworkClient _client; // 니가 만든 클래스

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

        _client = MockUse
            ? new MockNetworkClient()
            : new NetworkClient();

        if (mainBoard == null)
        {
            mainBoard = FindAnyObjectByType<MainBoard>();
        }
    }

    // : ä û Ѵ.
    public void RequestWord(string msg)
    {
        if (_client is NetworkClient networkClient)
        {
            networkClient.SendWord(msg);
        }
    }

    // :  ǥ û Ѵ.
    public bool RequestPosition(uint x, uint y)
    {
        return _client.SendPosition(x, y);
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
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.OnConnectSuccess();
                    }
                    break;
                }
            case PacketType.S2C_MatchFound:
                {
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.OnMatchFound();
                    }
                    break;
                }
            case PacketType.S2C_MatchFail:
                {
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.OnMatchFail();
                    }
                    break;
                }
            case PacketType.S2C_PlaceStoneAck:
                {
                    bool success;
                    uint x;
                    uint y;
                    uint stone;

                    if (packet.HasPlaceStoneAckData)
                    {
                        success = packet.PlaceStoneSuccess;
                        x = packet.PlaceX;
                        y = packet.PlaceY;
                        stone = packet.PlaceStone;
                    }
                    else
                    {
                        success = PacketSerializer.TryParsePlace(packet.Payload, out x, out y);
                        stone = GameManager.instance != null && GameManager.instance.isMyTurn ? 1u : 2u;
                    }

                    if (success)
                    {
                        if (mainBoard != null)
                        {
                            mainBoard.ApplyPlace(x, y, stone);
                        }
                    }
                    else if (GameManager.instance != null)
                    {
                        GameManager.instance.OnPlaceRejected("place rejected");
                    }
                    break;
                }
            case PacketType.S2C_Error:
                {
                    if (GameManager.instance != null)
                    {
                        GameManager.instance.OnPlaceRejected(PacketSerializer.ParseString(packet.Payload));
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
        if (_client is NetworkClient networkClient)
        {
            networkClient.Disconnect();
        }
    }

    // 역할: 매칭 요청을 보낸다(예시).
    public void RequestMatch()
    {
        _client.SendMatchRequest();
    }
}
