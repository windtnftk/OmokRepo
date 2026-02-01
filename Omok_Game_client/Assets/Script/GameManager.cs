using Assets.Script;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    None,           // 앱 켜진 직후
    Connecting,     // 서버 연결 시도중
    ConnectFail,    // 연결 실패
    Connected,      // 서버 연결 완료
    Matching,       // 매칭 요청중
    InGame,         // 게임 진행중
    GameOver        // 게임 종료
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
    //public bool isGameOver { get; private set; }
    public int isTurn { get; private set; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        isMyTurn = true;
        //isGameOver = false;
        isTurn = 0;
    }
    public bool CanProcessBoardClick()
    {
        // 게임 중 + 내 턴일 때만 클릭 허용
        return (State == GameState.InGame) && isMyTurn;
    }

    public void SetState(GameState s) => State = s;
    public void HandleBoardClick(Vector3 worldPoint)
    {
        if (PlaceSucces.Win == placeSucces)
        {
            Debug.Log("게임 종료 상태");
            return;
        }
        placeSucces = board.TryCreateStone(worldPoint);
        // 메인보드 설치 실패시 
        if (placeSucces == PlaceSucces.None) return;
        if (PlaceSucces.Win == placeSucces) // 게임 승리시
        {
            Debug.Log("성공");
            UiManager.GameOverUi();
        }
        else
        {
            ++isTurn;
            isMyTurn = !isMyTurn;
            UiManager.SetisTurn();
        }
    }
    //public void isGameOverSet(bool check)
    //{
    //    isGameOver = check;
    //}
    public void GameReLoad()
    {
        SceneManager.LoadScene("MainScene");
    }
}