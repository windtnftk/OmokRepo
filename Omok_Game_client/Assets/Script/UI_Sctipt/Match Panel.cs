using TMPro;
using UnityEngine;

public class MatchPanel: MonoBehaviour
{
    [SerializeField] private GameObject MatchingButton;
    [SerializeField] private GameObject MatchingStart;
    [SerializeField] private TextMeshProUGUI M_TextMeshPro;

    public void ButtonOn()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RequestMatch();
        }
    }

    public void ShowMatchingText()
    {
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
    }

    public void ShowMatchFailText()
    {
        if (M_TextMeshPro != null)
        {
            M_TextMeshPro.SetText("매칭 실패!");
        }
        if (MatchingButton != null)
        {
            MatchingButton.SetActive(true);
        }
    }
}
