using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_TextMeshPro;
    [SerializeField] private Image myImage;
    [SerializeField] private Sprite WhiteStone;
    [SerializeField] private Sprite BlackStone;
    [SerializeField] private GameObject GameoverPanel;
    [SerializeField] private GameObject ConnectingPanel;
    [SerializeField] private GameObject MatchPanel;

    public void SetisTurn()
    {
        string SettingWord = "TURN " + GameManager.instance.isTurn.ToString();
        m_TextMeshPro.SetText(SettingWord);
        if (GameManager.instance.isMyTurn == true)
        {
            myImage.sprite = BlackStone;
        }
        else
        {
            myImage.sprite = WhiteStone;
        }
    }
    public void GameOverUi()
    {
        GameoverPanel.SetActive(true);
    }
}
