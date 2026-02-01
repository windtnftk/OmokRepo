using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI m_TextMeshPro;
    [SerializeField] private Image myImage;
    [SerializeField] private Sprite WhiteStone;
    [SerializeField] private Sprite BlackStone;
    [SerializeField] private GameObject GameoverPanel;
    [SerializeField] private GameObject ConnectingPanel;
    [SerializeField] private GameObject MatchPanel;
    [SerializeField] private ConnectPanel connectPanel;
    [SerializeField] private MatchPanel matchPanel;
    [SerializeField] private string ServerIp = "127.0.0.1";
    [SerializeField] private int ServerPort = 9000;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RequestConnect()
    {
        GameManager.instance.SetState(GameState.Connecting);
        if (connectPanel != null)
        {
            connectPanel.ShowConnectingText();
        }
        if (NetworkManager.Instance != null)
        {
            bool success = NetworkManager.Instance.Connect(ServerIp, ServerPort);
            if (!success)
            {
                OnConnectFail();
            }
        }
    }

    public void RequestMatch()
    {
        GameManager.instance.SetState(GameState.Matching);
        if (matchPanel != null)
        {
            matchPanel.ShowMatchingText();
        }
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.RequestMatch();
        }
    }

    public void OnConnectSuccess()
    {
        if (ConnectingPanel != null)
        {
            ConnectingPanel.SetActive(false);
        }
        if (MatchPanel != null)
        {
            MatchPanel.SetActive(true);
        }
        GameManager.instance.SetState(GameState.Connected);
    }

    public void OnConnectFail()
    {
        if (connectPanel != null)
        {
            connectPanel.ShowConnectFailText();
        }
        GameManager.instance.SetState(GameState.ConnectFail);
    }

    public void OnMatchFound()
    {
        if (MatchPanel != null)
        {
            MatchPanel.SetActive(false);
        }
        GameManager.instance.SetState(GameState.InGame);
    }

    public void OnMatchFail()
    {
        if (matchPanel != null)
        {
            matchPanel.ShowMatchFailText();
        }
        GameManager.instance.SetState(GameState.Connected);
    }

    public void SetisTurn()
    {
        string SettingWord = "TURN " + GameManager.instance.isTurn.ToString();
        m_TextMeshPro.SetText(SettingWord);
        if (GameManager.instance.isMyTurn == true)
        {
            myImage.sprite = BlackStone;
        }
        else
        {
            myImage.sprite = WhiteStone;
        }
    }
    public void GameOverUi()
    {
        GameoverPanel.SetActive(true);
    }
}
