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
            M_TextMeshPro.SetText("Try Matching...");
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
            M_TextMeshPro.SetText("Matching false!");
        }
        if (MatchingButton != null)
        {
            MatchingButton.SetActive(true);
        }
    }

    public void ShowReadyText()
    {
        if (M_TextMeshPro != null)
        {
            M_TextMeshPro.SetText("Ready");
        }
        if (MatchingButton != null)
        {
            MatchingButton.SetActive(true);
        }
        if (MatchingStart != null)
        {
            MatchingStart.SetActive(false);
        }
    }
}
