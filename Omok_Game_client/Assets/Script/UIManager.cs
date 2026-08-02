using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI ConnectingText;
    [SerializeField] private TurnPanel turnPanel;
    [SerializeField] private UserPanel userPanel;
    [SerializeField] private Sprite blackStoneSprite;
    [SerializeField] private Sprite whiteStoneSprite;
    [SerializeField] private GameObject ConnectingPanel;
    [SerializeField] private GameObject MatchPanel;
    [SerializeField] private GameObject GamePanel;
    [SerializeField] private GameObject BoardObject;
    [SerializeField] private string ServerIp = "222.239.88.107";
    [SerializeField] private int ServerPort = 9000;

    private int approvedStoneCount;
    private uint cachedMyColor = 1u;
    private uint cachedOpponentColor = 2u;
    private bool isGameEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        ResetGamePanels();
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
        ShowMatchView(true);
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
        ResetGamePanels();
        ShowGameView();
        GameManager.instance.SetState(GameState.InGame);
        SetupMatchPanels();
    }

    private void ShowGameView()
    {
        if (ConnectingPanel != null)
        {
            ConnectingPanel.SetActive(false);
        }
        if (MatchPanel != null)
        {
            MatchPanel.SetActive(false);
        }
        if (GamePanel != null)
        {
            GamePanel.SetActive(true);
        }
        if (BoardObject != null)
        {
            BoardObject.SetActive(true);
        }
        SetGamePanelsActive(true);
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
        if (isGameEnded || GameManager.instance == null)
        {
            return;
        }

        approvedStoneCount = GameManager.instance.isTurn;
        uint currentTurnColor = GetCurrentTurnColor(approvedStoneCount);
        Sprite currentTurnStone = GetStoneSprite(currentTurnColor);
        bool isMyTurn = currentTurnColor == cachedMyColor;
        int turnNumber = approvedStoneCount + 1;

        if (turnPanel != null)
        {
            turnPanel.UpdateTurn(currentTurnStone, isMyTurn, turnNumber);
        }
    }

    public void GameOverUi(bool isWin, uint reasonCode, bool connectionAlive)
    {
        isGameEnded = true;
        uint winnerColor = isWin ? cachedMyColor : cachedOpponentColor;
        Sprite winnerStone = GetStoneSprite(winnerColor);
        string winnerText = winnerColor == 2u ? "WHITE\nWINS" : "BLACK\nWINS";

        if (turnPanel != null)
        {
            turnPanel.gameObject.SetActive(true);
            turnPanel.ShowGameResult(winnerStone, winnerText, approvedStoneCount);
        }
    }

    public void ReturnToMatchForRetry()
    {
        bool connectionAlive = NetworkManager.Instance != null && NetworkManager.Instance.IsConnected;
        ShowMatchView(connectionAlive);
        GameManager.instance.SetState(connectionAlive ? GameState.Connected : GameState.ConnectFail);
    }

    private void ShowMatchView(bool connectionAlive)
    {
        ResetGamePanels();

        if (connectionAlive)
        {
            if (ConnectingPanel != null)
            {
                ConnectingPanel.SetActive(false);
            }
            if (GamePanel != null)
            {
                GamePanel.SetActive(false);
            }
            if (BoardObject != null)
            {
                BoardObject.SetActive(false);
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
        }
        else
        {
            if (GamePanel != null)
            {
                GamePanel.SetActive(false);
            }
            if (BoardObject != null)
            {
                BoardObject.SetActive(false);
            }
            if (MatchPanel != null)
            {
                MatchPanel.SetActive(false);
            }
            if (ConnectingPanel != null)
            {
                ConnectingPanel.SetActive(true);
            }
        }
    }

    private void SetupMatchPanels()
    {
        if (GameManager.instance == null)
        {
            return;
        }

        approvedStoneCount = 0;
        isGameEnded = false;
        cachedMyColor = GameManager.instance.myColor == 2u ? 2u : 1u;
        cachedOpponentColor = cachedMyColor == 1u ? 2u : 1u;

        if (userPanel != null)
        {
            userPanel.SetupPlayerColor(GetStoneSprite(cachedMyColor));
        }

        if (turnPanel != null)
        {
            uint currentTurnColor = GetCurrentTurnColor(approvedStoneCount);
            turnPanel.SetupGame(GetStoneSprite(currentTurnColor), currentTurnColor == cachedMyColor, 1);
        }
    }

    private void ResetGamePanels()
    {
        approvedStoneCount = 0;
        isGameEnded = false;

        if (turnPanel != null)
        {
            turnPanel.ResetPanel();
        }
        if (userPanel != null)
        {
            userPanel.ResetPanel();
        }

        SetGamePanelsActive(false);
    }

    private void SetGamePanelsActive(bool isActive)
    {
        if (turnPanel != null)
        {
            turnPanel.gameObject.SetActive(isActive);
        }
        if (userPanel != null)
        {
            userPanel.gameObject.SetActive(isActive);
        }
    }

    private Sprite GetStoneSprite(uint stoneColor)
    {
        return stoneColor == 2u ? whiteStoneSprite : blackStoneSprite;
    }

    private uint GetCurrentTurnColor(int approvedStones)
    {
        return approvedStones % 2 == 0 ? 1u : 2u;
    }
}
