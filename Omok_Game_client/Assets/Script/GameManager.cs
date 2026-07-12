using Assets.Script;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    None,
    Connecting,
    ConnectFail,
    Connected,
    Matching,
    InGame,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public PlaceSucces placeSucces;
    [SerializeField] MainBoard board;
    [SerializeField] InputManager InputManager;
    [SerializeField] UIManager UiManager;

    public GameState State { get; private set; } = GameState.None;
    public bool isMyTurn { get; private set; }
    public int isTurn { get; private set; }
    public int currentRoomId { get; private set; }
    public uint myColor { get; private set; }
    public uint opponentColor { get; private set; }
    public bool IsPlaceRequestPending { get; private set; }
    private float pendingSince;
    private const float PlacePendingTimeoutSeconds = 2.0f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        isMyTurn = true;
        isTurn = 0;
        currentRoomId = 0;
        myColor = 1u;
        opponentColor = 2u;
        IsPlaceRequestPending = false;
        pendingSince = 0f;
    }

    private void Update()
    {
        if (IsPlaceRequestPending && (Time.time - pendingSince) > PlacePendingTimeoutSeconds)
        {
            OnPlaceRejected("timeout");
        }
    }

    public bool CanProcessBoardClick()
    {
        return (State == GameState.InGame) && isMyTurn && !IsPlaceRequestPending;
    }

    public void SetState(GameState s) => State = s;

    public void InitializeMatch(int roomId, uint assignedColor, bool assignedTurn)
    {
        if (board != null)
        {
            board.ResetBoard();
        }

        placeSucces = PlaceSucces.None;
        currentRoomId = roomId;
        myColor = assignedColor == 2u ? 2u : 1u;
        opponentColor = myColor == 1u ? 2u : 1u;
        isMyTurn = assignedTurn;
        isTurn = 0;
        IsPlaceRequestPending = false;
        pendingSince = 0f;
    }

    public void HandleBoardClick(Vector3 worldPoint)
    {
        if (State == GameState.GameOver)
        {
            return;
        }

        board.RequestPlace(worldPoint);
    }

    public void OnPlaceRequestSent()
    {
        IsPlaceRequestPending = true;
        pendingSince = Time.time;
    }

    public void OnPlaceApplied(PlaceSucces success)
    {
        IsPlaceRequestPending = false;
        pendingSince = 0f;
        placeSucces = success;
        ++isTurn;
        isMyTurn = !isMyTurn;
        UiManager.SetisTurn();
    }

    public void OnPlaceRejected(string reason = null)
    {
        IsPlaceRequestPending = false;
        pendingSince = 0f;
    }

    public void OnGameOver(int roomId, uint winnerColor, uint reasonCode, bool connectionAlive)
    {
        if (roomId != currentRoomId && currentRoomId != 0)
        {
            return;
        }

        IsPlaceRequestPending = false;
        pendingSince = 0f;
        currentRoomId = 0;
        isMyTurn = false;
        isTurn = 0;
        State = GameState.GameOver;

        bool isWin = reasonCode == 1u && winnerColor == myColor;
        if (UiManager != null)
        {
            UiManager.GameOverUi(isWin, reasonCode, connectionAlive);
        }
    }

    public void GameReLoad()
    {
        SceneManager.LoadScene("MainScene");
    }
}
