using UnityEngine;
using Assets.Script;
public class MainBoard : MonoBehaviour
{
    private BoardLogic logic;
    
    //[SerializeField]
    //private GameObject Stone;
    //[SerializeField]
    //private float StonePosition;
    //// Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        logic = new BoardLogic();
    }
    public Grid grid;
    public LayerMask boardMask;    // 보드만 맞추는 레이어
    [field: SerializeField] public GameObject BlackStonePrefab { get; private set; }
    [field: SerializeField] public GameObject WhiteStonePrefab { get; private set; }
    public PlaceSucces TryCreateStone(Vector3 worldPos)
    {
        PlaceSucces Succes;
        // 2) 보드 히트(정확성): Board 레이어만
        var hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, boardMask);
        if (hit.collider == null)
        {
            return PlaceSucces.None;
        }

        // 3) 월드 → 셀 좌표
        Vector3Int cell = grid.WorldToCell(worldPos);

        // 4) 19×19 범위 보정
        //cell.x = Mathf.Clamp(cell.x, 0, 18);
        //cell.y = Mathf.Clamp(cell.y, 0, 18);

        // 5) 점유 여부 확인(나중에 “그 칸에 돌 있어?” 기능의 핵심)
        //if (board[cell.x, cell.y] != Stone.None) return false;  // 이미 있으면 무시

        // 6) 셀 중심 월드 좌표(= 스냅 위치)

        // 7) 돌 생성 & 보드 상태 갱신
        Stone StoneCheck = GameManager.instance.isMyTurn ? Stone.Black : Stone.White;
        Succes = logic.PlaceStone(cell.x, cell.y, StoneCheck);
        if (Succes == PlaceSucces.None) // 실패시
        {
            Debug.Log("실패");
            return Succes;
        }
        var prefab = StoneCheck == Stone.Black ? BlackStonePrefab : WhiteStonePrefab;
        Instantiate(prefab, grid.GetCellCenterWorld(cell), Quaternion.identity);
        return Succes;
        //board[cell.x, cell.y] = GameManager.instance.isMyTurn ? StoneSpawner.Black : StoneSpawner.White;
        //TryGameEnd(cell.x, cell.y);




    }
}
    // tryGameEnd 를 반환값을 어케함? 반환값을 현재 둔 돌의 색깔을 기준으로 하죠
    // ㄴㄴ 근본적인 문제에서 접근해야됨, 만약 돌을 두고 승리 해는지 체크할때 백돌이 승리
    // 했어요 하면 누가 받아줄거냐? -> GameManager 가 받아야지 그럼 그걸 통해서 uiManager 호출하고
    // 그럼 반환값을 줘야되나?

    // 5목 체크 함수
//    public void TryGameEnd(int x, int y)
//    {
//        StoneSpawner why = board[x, y];
//        for (int time = 0; time <= 7; ++time) // 대각선 및 가로세로 체크개수
//        {
//            Debug.Log(time + " 타임");
//            for (int check = 1; check <= 4; ++check) // 돌이 5개 연속으로 두어야 됨
//            {
//                Debug.Log(check + " 체크");
//                if (SelectStone(x,y,check,time))
//                {
//                    if (check == 4) // 돌이 연속 5개 되었는걸 체크 됨
//                    {
//                        GameManager.instance.isGameOverSet(true);
//                    }
//                    continue; 
//                }
//                break;
//            }
//        }
//    }
//    //
//    public bool SelectStone(int x, int y, int check, int time)
//    {
//        switch (time)
//        {
//            case 0: return board[x, y] == board[x - check, y - check];
//            case 1: return board[x, y] == board[x + check, y - check];
//            case 2: return board[x, y] == board[x - check, y + check];
//            case 3: return board[x, y] == board[x + check, y + check];
//            case 4: return board[x, y] == board[x - check, y];
//            case 5: return board[x, y] == board[x + check, y];
//            case 6: return board[x, y] == board[x, y - check];
//            case 7: return board[x, y] == board[x, y + check];
//            default: return false;
//        }
//    }
//}
