using UnityEngine;
using UnityEngine.UI;

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
