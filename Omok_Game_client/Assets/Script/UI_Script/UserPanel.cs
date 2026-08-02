using UnityEngine;
using UnityEngine.UI;

public class UserPanel : MonoBehaviour
{
    [SerializeField] private Image myColorImage;

    public void SetupPlayerColor(Sprite playerStone)
    {
        if (myColorImage != null)
        {
            myColorImage.sprite = playerStone;
        }
    }

    public void ResetPanel()
    {
        if (myColorImage != null)
        {
            myColorImage.sprite = null;
        }
    }
}
