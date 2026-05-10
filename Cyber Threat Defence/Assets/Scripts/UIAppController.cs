using UnityEngine;

// Toggles the messaging panel open/closed, and hides the connections layer while the
// inbox is open so it doesn't render on top of the panel.
public class UIAppController : MonoBehaviour
{
    public GameObject messagingPanel;

    public void OpenMessagingApp()
    {
        messagingPanel.SetActive(true);
        SetConnectionsVisible(false);
    }

    public void CloseMessagingApp()
    {
        messagingPanel.SetActive(false);
        SetConnectionsVisible(true);
    }

    private static void SetConnectionsVisible(bool visible)
    {
        RectTransform cp = ConnectionManager.Instance?.ConnectionsParent;
        if (cp != null) cp.gameObject.SetActive(visible);
    }
}