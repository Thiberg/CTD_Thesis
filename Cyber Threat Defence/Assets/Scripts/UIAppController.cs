using UnityEngine;

public class UIAppController : MonoBehaviour
{
    public GameObject messagingPanel;

    public void OpenMessagingApp()
    {
        messagingPanel.SetActive(true);
    }

    public void CloseMessagingApp()
    {
        messagingPanel.SetActive(false);
    }
}