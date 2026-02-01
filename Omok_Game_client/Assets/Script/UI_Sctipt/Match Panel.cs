using TMPro;
using UnityEngine;

public class MatchPanel: MonoBehaviour
{
    [SerializeField] private GameObject MatchingButton;
    [SerializeField] private GameObject MatchingStart;
    [SerializeField] private TextMeshProUGUI M_TextMeshPro;

    bool connected = false;
    public void ButtonOn()
    {
        GameManager.instance.SetState(GameState.Matching);
        if (M_TextMeshPro != null)
        {
            M_TextMeshPro.SetText("매칭중...");
        }
        if (MatchingButton != null)
        {
            MatchingButton.SetActive(false);
        }
        if (MatchingStart != null)
        {
            MatchingStart.SetActive(true);
        }
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.RequestMatch();
        }
    }

    public void HandleMatchFail()
    {
        if (M_TextMeshPro != null)
        {
            M_TextMeshPro.SetText("매칭 실패!");
        }
        if (MatchingButton != null)
        {
            MatchingButton.SetActive(true);
        }
        GameManager.instance.SetState(GameState.Connected);
    }

    public void HandleMatchSuccess()
    {
        gameObject.SetActive(false);
        GameManager.instance.SetState(GameState.InGame);
    }
}
