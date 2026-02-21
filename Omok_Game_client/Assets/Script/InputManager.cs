using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private bool _clickRequested;

    // InputAction 이벤트에서 호출(콜백)
    public void OnClickAction()
    {
        _clickRequested = true; // 여기서 UI 체크하지 말기
    }

    private void Update()
    {
        if (!_clickRequested) return;
        _clickRequested = false;

        // 0) UI 위 클릭이면 무시 (이번 프레임 UI 상태 기준이라 안전)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 1) 보드 입력 가능한 상태인지 체크
        if (!GameManager.instance.CanProcessBoardClick())
            return;

        // 2) 스크린 → 월드
        if (Camera.main == null || Mouse.current == null) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        worldPos.z = 0f;

        GameManager.instance.HandleBoardClick(worldPos);
    }
}
