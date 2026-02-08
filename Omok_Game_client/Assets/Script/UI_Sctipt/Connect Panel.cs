using TMPro;
using UnityEngine;

public class ConnectPanel : MonoBehaviour
{
    [SerializeField] private GameObject ConnectingButton;
    [SerializeField] private GameObject ConnectingStart;
    [SerializeField] private TextMeshProUGUI C_TextMeshPro;

    public void ButtonOn()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RequestConnect();
        }
    }

    public void ShowConnectingText()
    {
        if (ConnectingButton != null)
        {
            ConnectingButton.SetActive(false);
        }
        if (ConnectingStart != null)
        {
            ConnectingStart.SetActive(true);
        }
        if (C_TextMeshPro != null)
        {
            C_TextMeshPro.SetText("Try Connecting...");
        }
    }

    public void ShowConnectFailText()
    {
        if (C_TextMeshPro != null)
        {
            C_TextMeshPro.SetText("Connect Fail!");
        }
        if (ConnectingButton != null)
        {
            ConnectingButton.SetActive(true);
        }
    }
}
