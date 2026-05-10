using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

// Right-side info panel that appears on node click. Shows live stats (HP, security,
// type-specific status) and exposes Sell, Upgrade, and Change Password actions.
// Auto-hides when the player clicks outside the panel.
public class NodeInspectorPanel : MonoBehaviour
{
    [Header("Panel Root")]
    public GameObject panelRoot;

    [Header("Node Icons (one per type — shown/hidden)")]
    public GameObject iconService;
    public GameObject iconServer;
    public GameObject iconDatabase;
    public GameObject iconFirewall;

    [Header("Text Fields")]
    public TextMeshProUGUI nodeName;
    public TextMeshProUGUI nodeLevel;
    public TextMeshProUGUI nodeDescription;

    [Header("Live Stats")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI securityText;
    public TextMeshProUGUI statusText;

    [Header("Buttons")]
    public Button upgradeButton;
    public Button sellButton;
    public Button changePasswordButton;

    private const int ChangePasswordCost = 50;

    private DragableItem currentNode;
    private PlacementSpot currentSpot;
    private RectTransform panelRect;

    private TextMeshProUGUI sellLabel;
    private TextMeshProUGUI upgradeLabel;

    private void Start()
    {
        panelRect = panelRoot.GetComponent<RectTransform>();

        if (sellButton != null)    sellLabel    = sellButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (upgradeButton != null) upgradeLabel = upgradeButton.GetComponentInChildren<TextMeshProUGUI>(true);

        panelRoot.SetActive(false);
        sellButton.onClick.AddListener(OnSellClicked);
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
        if (changePasswordButton != null)
            changePasswordButton.onClick.AddListener(OnChangePasswordClicked);
    }

    private void Update()
    {
        if (!panelRoot.activeSelf) return;

        RefreshStats();

        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        if (!RectTransformUtility.RectangleContainsScreenPoint(panelRect, Mouse.current.position.ReadValue(), null))
            Hide();
    }

    public void Show(DragableItem node, PlacementSpot spot)
    {
        currentNode = node;
        currentSpot = spot;

        HideAllIcons();

        if (node.nodeData != null)
        {
            nodeName.text        = node.nodeData.nodeName;
            nodeDescription.text = node.nodeData.description;
            ShowIconForType(node.nodeData.nodeType);
        }
        else
        {
            nodeName.text        = node.gameObject.name;
            nodeLevel.text       = "";
            nodeDescription.text = "";
        }

        RefreshStats();
        panelRoot.SetActive(true);
    }

    private void RefreshStats()
    {
        if (currentNode == null || currentNode.nodeData == null) return;

        if (nodeLevel != null)
            nodeLevel.text = "Tier: " + (currentNode.nodeData.tier + currentNode.UpgradeLevel);

        if (hpText != null)
            hpText.text = $"HP: {currentNode.CurrentHealth}/{currentNode.CurrentMaxHealth}";

        if (securityText != null)
            securityText.text = "Security: " + currentNode.CurrentSecurityLevel;

        if (sellLabel != null)
            sellLabel.text = $"Sell  ${currentNode.GetSellRefund()}";

        if (upgradeLabel != null && upgradeButton != null)
        {
            if (currentNode.IsCompromised)
            {
                upgradeLabel.text = "Compromised";
                upgradeButton.interactable = false;
            }
            else if (!currentNode.CanUpgrade)
            {
                upgradeLabel.text = "Maxed";
                upgradeButton.interactable = false;
            }
            else
            {
                int cost = currentNode.GetUpgradeCost();
                upgradeLabel.text = $"Upgrade  ${cost}";
                upgradeButton.interactable = GameManager.Instance != null && GameManager.Instance.CanAfford(cost);
            }
        }

        if (changePasswordButton != null)
        {
            int sharedWith = CredentialManager.Instance != null
                ? CredentialManager.Instance.GetGroupMembers(currentNode).Count
                : 0;
            bool relevant = !currentNode.IsCompromised && (sharedWith > 0 || currentNode.IsLeaking);

            changePasswordButton.gameObject.SetActive(relevant);

            if (relevant)
            {
                bool canPay = GameManager.Instance != null && GameManager.Instance.CanAfford(ChangePasswordCost);
                changePasswordButton.interactable = canPay;
            }
        }

        if (statusText == null) return;

        if (currentNode.IsCompromised)
        {
            statusText.text = "<color=#FF5555>COMPROMISED</color>";
            return;
        }

        switch (currentNode.nodeData.nodeType)
        {
            case NodeType.Service:
                bool intact = ConnectionManager.Instance != null
                              && ConnectionManager.Instance.IsChainIntact(currentNode);
                statusText.text = intact
                    ? $"Revenue: ${currentNode.CurrentRevenuePerSecond:0.##}/s"
                    : $"Revenue: ${currentNode.CurrentRevenuePerSecond:0.##}/s (no chain)";
                break;

            case NodeType.Server:
            {
                int services = ConnectionManager.Instance != null
                    ? ConnectionManager.Instance.GetConnectedNodes(currentNode, NodeType.Service).Count
                    : 0;
                string upkeepInfo = currentNode.CurrentUpkeepPerSecond > 0f
                    ? $"  |  Upkeep: ${currentNode.CurrentUpkeepPerSecond:0.##}/s"
                    : "";
                statusText.text = "Connected services: " + services + upkeepInfo;
                break;
            }

            case NodeType.Database:
            {
                int servers = ConnectionManager.Instance != null
                    ? ConnectionManager.Instance.GetConnectedNodes(currentNode, NodeType.Server).Count
                    : 0;
                string upkeepInfo = currentNode.CurrentUpkeepPerSecond > 0f
                    ? $"  |  Upkeep: ${currentNode.CurrentUpkeepPerSecond:0.##}/s"
                    : "";
                statusText.text = "Connected servers: " + servers + upkeepInfo;
                break;
            }

            case NodeType.Firewall:
            {
                int connections = ConnectionManager.Instance != null
                    ? ConnectionManager.Instance.GetConnectionCount(currentNode)
                    : 0;
                float upkeep = currentNode.CurrentUpkeepPerSecond * connections;
                statusText.text = $"Upkeep: ${currentNode.CurrentUpkeepPerSecond:0.##}/s × {connections} = ${upkeep:0.##}/s";
                break;
            }
        }
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
        currentNode = null;
        currentSpot = null;
    }

    private void HideAllIcons()
    {
        if (iconService)  iconService.SetActive(false);
        if (iconServer)   iconServer.SetActive(false);
        if (iconDatabase) iconDatabase.SetActive(false);
        if (iconFirewall) iconFirewall.SetActive(false);
    }

    private void ShowIconForType(NodeType type)
    {
        switch (type)
        {
            case NodeType.Service:  if (iconService)  iconService.SetActive(true);  break;
            case NodeType.Server:   if (iconServer)   iconServer.SetActive(true);   break;
            case NodeType.Database: if (iconDatabase) iconDatabase.SetActive(true); break;
            case NodeType.Firewall: if (iconFirewall) iconFirewall.SetActive(true); break;
        }
    }

    private void OnSellClicked()
    {
        if (currentNode == null) return;

        if (currentNode.nodeData != null)
        {
            GameManager.Instance?.AddBalance(currentNode.GetSellRefund());
        }

        GameManager.Instance?.UnregisterNode(currentNode);
        Destroy(currentNode.gameObject);
        Hide();
    }

    private void OnUpgradeClicked()
    {
        if (currentNode == null) return;
        if (currentNode.IsCompromised) return;
        if (!currentNode.CanUpgrade) return;

        int cost = currentNode.GetUpgradeCost();
        if (GameManager.Instance == null || !GameManager.Instance.CanAfford(cost)) return;

        GameManager.Instance.DeductBalance(cost);
        currentNode.Upgrade();
    }

    private void OnChangePasswordClicked()
    {
        if (currentNode == null) return;
        if (currentNode.IsCompromised) return;
        if (GameManager.Instance == null || !GameManager.Instance.CanAfford(ChangePasswordCost)) return;

        GameManager.Instance.DeductBalance(ChangePasswordCost);
        CredentialManager.Instance?.SwapToNewCredential(currentNode);
        CredentialLeakManager.Instance?.StopLeak(currentNode);
    }
}