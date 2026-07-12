using NetworkClientApp;
using Protocol;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }
    public bool MockUse = false;
    [SerializeField] private MainBoard mainBoard;

    private INetworkClient _client; // 니가 만든 클래스

    public bool IsConnected => _client != null && _client.IsConnected;

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
                    if (PacketSerializer.TryParseMatchFound(packet.Payload, out int roomId, out uint myColor, out uint isMyTurn))
                    {
                        if (GameManager.instance != null)
                        {
                            GameManager.instance.InitializeMatch(roomId, myColor, isMyTurn != 0u);
                        }

                        if (UIManager.Instance != null)
                        {
                            UIManager.Instance.OnMatchFound();
                        }
                    }
                    else if (GameManager.instance != null)
                    {
                        GameManager.instance.OnPlaceRejected("bad match payload");
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
                        success = PacketSerializer.TryParsePlaceStoneAck(packet.Payload, out x, out y, out stone);
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
            case PacketType.S2C_GameOver:
                {
                    if (PacketSerializer.TryParseGameOver(packet.Payload, out int roomId, out uint winnerColor, out uint reasonCode))
                    {
                        if (GameManager.instance != null)
                        {
                            GameManager.instance.OnGameOver(roomId, winnerColor, reasonCode, IsConnected);
                        }
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
