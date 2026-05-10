using UnityEngine;
using UnityEngine.UI;

// Tiny badge on the messaging app icon. Toggles its Image renderer based on
// MessageManager.UnreadCount (toggling the Image rather than the GameObject avoids
// the chicken-and-egg of OnEnable not firing on inactive GameObjects).
[RequireComponent(typeof(Image))]
public class NotificationBadge : MonoBehaviour
{
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        MessageManager.OnInboxChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        MessageManager.OnInboxChanged -= Refresh;
    }

    private void Refresh()
    {
        if (image == null) return;
        bool show = MessageManager.Instance != null && MessageManager.Instance.UnreadCount > 0;
        image.enabled = show;
    }
}
