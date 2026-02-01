using TMPro;
using UnityEngine;

public class ConnectPanel : MonoBehaviour
{
    [SerializeField] private GameObject ConnectingButton;
    [SerializeField] private GameObject ConnectingStart;
    [SerializeField] private TextMeshProUGUI C_TextMeshPro;
    [SerializeField] private GameObject MatchPanel;
    [SerializeField] private string ServerIp = "127.0.0.1";
    [SerializeField] private int ServerPort = 9000;

    bool connected = false;
    public void ButtonOn()
    {
        GameManager.instance.SetState(GameState.Connecting);
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
            C_TextMeshPro.SetText("연결중...");
        }
        if (NetworkManager.Instance != null)
        {
            bool success = NetworkManager.Instance.Connect(ServerIp, ServerPort);
            if (!success)
            {
                HandleConnectFail();
            }
        }
    }

    public void HandleConnectFail()
    {
        if (C_TextMeshPro != null)
        {
            C_TextMeshPro.SetText("연결 실패!");
        }
        if (ConnectingButton != null)
        {
            ConnectingButton.SetActive(true);
        }
        GameManager.instance.SetState(GameState.ConnectFail);
    }

    public void HandleConnectSuccess()
    {
        gameObject.SetActive(false);
        if (MatchPanel != null)
        {
            MatchPanel.SetActive(true);
        }
        GameManager.instance.SetState(GameState.Connected);
    }
}
