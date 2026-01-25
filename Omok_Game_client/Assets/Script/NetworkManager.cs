using NetworkClientApp;
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

    // 역할: 서버 연결을 시작한다.
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
        //_client.SendMatchRequest(); // 네 함수명에 맞춰서
    }
}
