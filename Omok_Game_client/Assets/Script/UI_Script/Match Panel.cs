using TMPro;
using UnityEngine;

public class MatchPanel : MonoBehaviour
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
        if (MatchingButton != null)
        {
            MatchingButton.SetActive(false);
        }

        if (MatchingStart != null)
        {
            MatchingStart.SetActive(true);
        }

        if (M_TextMeshPro != null)
        {
            M_TextMeshPro.SetText("SEARCHING FOR OPPONENT...");
        }
    }

    public void ShowMatchFailText()
    {
        if (M_TextMeshPro != null)
        {
            M_TextMeshPro.SetText("MATCHING FAILED");
        }

        if (MatchingStart != null)
        {
            MatchingStart.SetActive(false);
        }

        if (MatchingButton != null)
        {
            MatchingButton.SetActive(true);
        }
    }

    public void ShowReadyText()
    {
        if (MatchingStart != null)
        {
            MatchingStart.SetActive(false);
        }

        if (MatchingButton != null)
        {
            MatchingButton.SetActive(true);
        }

        if (M_TextMeshPro != null)
        {
            M_TextMeshPro.SetText("READY");
        }
    }
}
