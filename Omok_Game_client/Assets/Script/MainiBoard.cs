using UnityEngine;
using Assets.Script;

public class MainBoard : MonoBehaviour
{
    private BoardLogic logic;

    private void Awake()
    {
        logic = new BoardLogic();
    }

    public Grid grid;
    public LayerMask boardMask;
    [field: SerializeField] public GameObject BlackStonePrefab { get; private set; }
    [field: SerializeField] public GameObject WhiteStonePrefab { get; private set; }

    public void RequestPlace(Vector3 worldPos)
    {
        var hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, boardMask);
        if (hit.collider == null)
        {
            return;
        }

        Vector3Int cell = grid.WorldToCell(worldPos);
        if (!logic.IsInBounds(cell.x, cell.y))
        {
            return;
        }

        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.RequestPosition((uint)cell.x, (uint)cell.y);
            if (GameManager.instance != null)
            {
                GameManager.instance.OnPlaceRequestSent();
            }
        }
    }

    public void ApplyPlace(uint x, uint y, uint stone)
    {
        Stone stoneType = stone == 2u ? Stone.White : Stone.Black;
        PlaceSucces succes = logic.PlaceStone((int)x, (int)y, stoneType);
        if (succes == PlaceSucces.None)
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.OnPlaceRejected();
            }
            return;
        }

        var prefab = stoneType == Stone.Black ? BlackStonePrefab : WhiteStonePrefab;
        Vector3Int cell = new Vector3Int((int)x, (int)y, 0);
        Instantiate(prefab, grid.GetCellCenterWorld(cell), Quaternion.identity);

        if (GameManager.instance != null)
        {
            GameManager.instance.OnPlaceApplied(succes);
        }
    }
}
