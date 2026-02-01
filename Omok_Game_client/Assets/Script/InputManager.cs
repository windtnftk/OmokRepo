using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    //[field: SerializeField] public MainBoard Board { get; private set; }
    //int[,] x = new int[5, 5];
    public void OnClickAction()
    {
        // 0) UI 위 클릭이면 무시 (패널 덮여있을 때도 안전)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 1) 보드 입력 가능한 상태인지 체크
        if (!GameManager.instance.CanProcessBoardClick())
            return;

        // 1) 스크린 → 월드
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        worldPos.z = 0f;
        Debug.Log(worldPos.x);
        Debug.Log(worldPos.y);
        GameManager.instance.HandleBoardClick(worldPos);

    }
}
