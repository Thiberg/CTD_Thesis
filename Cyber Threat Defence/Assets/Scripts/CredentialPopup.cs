using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CredentialPopup : MonoBehaviour
{
    public static CredentialPopup Instance { get; private set; }

    [Header("Panel")]
    public GameObject panelRoot;

    [Header("Buttons")]
    public Button newPasswordButton;
    public Button lastPasswordButton;

    [Header("Text")]
    public TextMeshProUGUI newPasswordLabel;
    public TextMeshProUGUI lastPasswordLabel;
    public TextMeshProUGUI warningText;

    private DragableItem pendingNode;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        panelRoot.SetActive(false);
        newPasswordButton.onClick.AddListener(OnNewPassword);
        lastPasswordButton.onClick.AddListener(OnLastPassword);
    }

    public void Show(DragableItem node)
    {
        pendingNode = node;

        bool canAffordNew = GameManager.Instance == null || GameManager.Instance.CanAfford(50);
        bool hasLast = CredentialManager.Instance != null && CredentialManager.Instance.HasLastCredential;

        newPasswordButton.interactable = canAffordNew;
        newPasswordLabel.text = canAffordNew ? "Generate New Password  (-$50)" : "Generate New Password  (-$50)\n<size=70%>Can't afford</size>";

        lastPasswordButton.interactable = hasLast;
        lastPasswordLabel.text = hasLast ? "Use Last Password  (Free)" : "Use Last Password  (Free)\n<size=70%>No previous password</size>";

        // Edge case: can't afford new AND no last password — assign unique for free
        if (!canAffordNew && !hasLast)
        {
            AssignFallbackCredential(node);
            return;
        }

        if (warningText != null)
            warningText.text = "Shared passwords cascade a breach across all linked nodes.";

        panelRoot.SetActive(true);
    }

    private void OnNewPassword()
    {
        if (pendingNode == null) return;
        CredentialManager.Instance.AssignNewCredential(pendingNode);
        panelRoot.SetActive(false);
        pendingNode = null;
    }

    private void OnLastPassword()
    {
        if (pendingNode == null) return;
        CredentialManager.Instance.AssignLastCredential(pendingNode);
        panelRoot.SetActive(false);
        pendingNode = null;
    }

    private void AssignFallbackCredential(DragableItem node)
    {
        // First node ever with no balance for credentials — assign unique for free
        CredentialManager.Instance.AssignNewCredential(node);
        GameManager.Instance?.AddBalance(50f); // refund the $50 silently
    }
}
