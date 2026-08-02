using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private Image stoneImage;
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private GameObject backToMatchButton;

    public void SetupGame(Sprite currentTurnStone, bool isMyTurn, int turnNumber)
    {
        SetTurnState(currentTurnStone, isMyTurn, turnNumber);
        SetBackToMatchButtonActive(false);
    }

    public void UpdateTurn(Sprite currentTurnStone, bool isMyTurn, int turnNumber)
    {
        SetTurnState(currentTurnStone, isMyTurn, turnNumber);
        SetBackToMatchButtonActive(false);
    }

    public void ShowGameResult(Sprite winnerStone, string winnerText, int finalTurn)
    {
        if (mainText != null)
        {
            mainText.SetText(winnerText);
        }
        if (stoneImage != null)
        {
            stoneImage.sprite = winnerStone;
        }
        if (turnText != null)
        {
            turnText.SetText("TURN " + finalTurn);
        }

        SetBackToMatchButtonActive(true);
    }

    public void ResetPanel()
    {
        if (mainText != null)
        {
            mainText.SetText(string.Empty);
        }
        if (stoneImage != null)
        {
            stoneImage.sprite = null;
        }
        if (turnText != null)
        {
            turnText.SetText(string.Empty);
        }

        SetBackToMatchButtonActive(false);
    }

    private void SetTurnState(Sprite currentTurnStone, bool isMyTurn, int turnNumber)
    {
        if (mainText != null)
        {
            mainText.SetText(isMyTurn ? "YOUR\nTURN" : "WAIT");
        }
        if (stoneImage != null)
        {
            stoneImage.sprite = currentTurnStone;
        }
        if (turnText != null)
        {
            turnText.SetText("TURN " + turnNumber);
        }
    }

    private void SetBackToMatchButtonActive(bool isActive)
    {
        if (backToMatchButton != null)
        {
            backToMatchButton.SetActive(isActive);
        }
    }
}
