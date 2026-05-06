using UnityEngine;
using UnityEngine.UI;

public class DirectionPopup : MonoBehaviour
{
    public static DirectionPopup Instance { get; private set; }

    [Header("Panel")]
    public GameObject panelRoot;

    [Header("Buttons")]
    public Button twoWayButton;  // "Two-Way  ↔"
    public Button oneWayButton;  // "One-Way  →  (source → target)"

    private NodeConnection pendingConnection;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        panelRoot.SetActive(false);
        twoWayButton.onClick.AddListener(() => Finalize(ConnectionDirection.TwoWay));
        oneWayButton.onClick.AddListener(() => Finalize(ConnectionDirection.AtoB));
    }

    public void Show(NodeConnection connection)
    {
        pendingConnection = connection;
        panelRoot.SetActive(true);
    }

    private void Finalize(ConnectionDirection direction)
    {
        if (pendingConnection == null) return;

        pendingConnection.Direction = direction;

        // Tint the line to reflect direction choice
        if (pendingConnection.LineObject != null)
        {
            Image img = pendingConnection.LineObject.GetComponent<Image>();
            if (img != null && ConnectionManager.Instance != null)
                img.color = direction == ConnectionDirection.TwoWay
                    ? ConnectionManager.Instance.TwoWayColor
                    : ConnectionManager.Instance.OneWayColor;
        }

        pendingConnection = null;
        panelRoot.SetActive(false);
    }
}
