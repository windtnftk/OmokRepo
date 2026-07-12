using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI TurnText;
    [SerializeField] private TextMeshProUGUI YourTurnText;
    [SerializeField] private TextMeshProUGUI ConnectingText;
    [SerializeField] private Image myImage;
    [SerializeField] private Sprite WhiteStone;
    [SerializeField] private Sprite BlackStone;
    [SerializeField] private GameObject GameoverPanel;
    [SerializeField] private GameObject ConnectingPanel;
    [SerializeField] private GameObject MatchPanel;
    [SerializeField] private string ServerIp = "222.239.88.107";
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
        if (NetworkManager.Instance != null && NetworkManager.Instance.IsConnected)
        {
            OnConnectSuccess();
            return;
        }

        GameManager.instance.SetState(GameState.Connecting);
        if (ConnectingPanel != null)
        {
            ConnectPanel panel = ConnectingPanel.GetComponent<ConnectPanel>();
            if (panel != null)
            {
                panel.ShowConnectingText();
            }
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
        if (GameManager.instance != null && GameManager.instance.State == GameState.Matching)
        {
            return;
        }

        GameManager.instance.SetState(GameState.Matching);
        if (MatchPanel != null)
        {
            MatchPanel panel = MatchPanel.GetComponent<MatchPanel>();
            if (panel != null)
            {
                panel.ShowMatchingText();
            }
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
        if (ConnectingPanel != null)
        {
            ConnectPanel panel = ConnectingPanel.GetComponent<ConnectPanel>();
            if (panel != null)
            {
                panel.ShowConnectFailText();
            }
        }
        GameManager.instance.SetState(GameState.ConnectFail);
    }

    public void OnMatchFound()
    {
        if (GameoverPanel != null)
        {
            GameoverPanel.SetActive(false);
        }
        if (MatchPanel != null)
        {
            MatchPanel.SetActive(false);
        }
        GameManager.instance.SetState(GameState.InGame);
        SetisTurn();
    }

    public void OnMatchFail()
    {
        if (MatchPanel != null)
        {
            MatchPanel panel = MatchPanel.GetComponent<MatchPanel>();
            if (panel != null)
            {
                panel.ShowMatchFailText();
            }
        }
        GameManager.instance.SetState(GameState.Connected);
    }

    public void SetisTurn()
    {
        if (TurnText != null)
        {
            TurnText.SetText("TURN " + GameManager.instance.isTurn.ToString());
        }
        if (YourTurnText != null)
        {
            YourTurnText.SetText(GameManager.instance.isMyTurn ? "Your Turn" : "Opponent's Turn");
        }
        if (myImage != null)
        {
            myImage.sprite = GameManager.instance.myColor == 2u ? WhiteStone : BlackStone;
        }
    }

    public void GameOverUi(bool isWin, uint reasonCode, bool connectionAlive)
    {
        if (GameoverPanel != null)
        {
            GameoverPanel.SetActive(true);
        }
        StartCoroutine(ReturnAfterGameOver(connectionAlive));
    }

    private IEnumerator ReturnAfterGameOver(bool connectionAlive)
    {
        yield return new WaitForSeconds(2.0f);

        if (GameoverPanel != null)
        {
            GameoverPanel.SetActive(false);
        }

        if (connectionAlive && NetworkManager.Instance != null && NetworkManager.Instance.IsConnected)
        {
            if (ConnectingPanel != null)
            {
                ConnectingPanel.SetActive(false);
            }
            if (MatchPanel != null)
            {
                MatchPanel.SetActive(true);
                MatchPanel panel = MatchPanel.GetComponent<MatchPanel>();
                if (panel != null)
                {
                    panel.ShowReadyText();
                }
            }
            GameManager.instance.SetState(GameState.Connected);
        }
        else
        {
            if (MatchPanel != null)
            {
                MatchPanel.SetActive(false);
            }
            if (ConnectingPanel != null)
            {
                ConnectingPanel.SetActive(true);
            }
            GameManager.instance.SetState(GameState.ConnectFail);
        }
    }
}
