using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    //[field: SerializeField] public MainBoard Board { get; private set; }
    int[,] x = new int[5, 5];
    public void OnClickAction()
    {
        // 0) UI 클릭 무시
        // (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        // 1) 스크린 → 월드
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        worldPos.z = 0f;
        Debug.Log(worldPos.x);
        Debug.Log(worldPos.y);
        GameManager.instance.HandleBoardClick(worldPos);

    }
}
