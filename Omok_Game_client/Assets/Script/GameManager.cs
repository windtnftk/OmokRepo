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

    public void HandleBoardClick(Vector3 worldPoint)
    {
        if (PlaceSucces.Win == placeSucces)
        {
            Debug.Log("게임 종료 상태");
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

        if (PlaceSucces.Win == placeSucces)
        {
            Debug.Log("승리");
            UiManager.GameOverUi();
        }
        else
        {
            ++isTurn;
            isMyTurn = !isMyTurn;
            UiManager.SetisTurn();
        }
    }

    public void OnPlaceRejected(string reason = null)
    {
        IsPlaceRequestPending = false;
        pendingSince = 0f;
    }

    public void GameReLoad()
    {
        SceneManager.LoadScene("MainScene");
    }
}
