using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] MainBoard board;
    [SerializeField] InputManager InputManager;
    [SerializeField] UIManager UiManager;

    public bool isBlackTurn { get; private set; }
    public bool isGameOver { get; private set; }
    public int isTurn { get; private set; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        isBlackTurn = true;
        isGameOver = false;
        isTurn = 0;
    }
    public void HandleBoardClick(Vector3 worldPoint)
    {
        if (isGameOver)
        {
            Debug.Log("게임 종료 상태");
            return;
        }
        // 메인보드 설치 및 성공시 
        if (board.TryCreateStone(worldPoint) && isGameOver == false)
        {
            ++isTurn;
            isBlackTurn = !isBlackTurn;
            UiManager.SetisTurn();
            
        }
        else if (isGameOver) // 게임 종료 확인 및 ui 호출
        {
            Debug.Log("성공");
            UiManager.GameOverUi();
        }
        else 
        { 
          // 돌 두기 실패시
        }
    }
    public void isGameOverSet(bool check)
    {
        isGameOver = check;
    }
    public void GameReLoad()
    {
        SceneManager.LoadScene("MainScene");
    }
}