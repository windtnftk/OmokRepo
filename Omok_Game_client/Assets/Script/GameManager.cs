using Assets.Script;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public PlaceSucces placeSucces;
    [SerializeField] MainBoard board;
    [SerializeField] InputManager InputManager;
    [SerializeField] UIManager UiManager;

    public bool isBlackTurn { get; private set; }
    //public bool isGameOver { get; private set; }
    public int isTurn { get; private set; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        isBlackTurn = true;
        //isGameOver = false;
        isTurn = 0;
    }
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
            isBlackTurn = !isBlackTurn;
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