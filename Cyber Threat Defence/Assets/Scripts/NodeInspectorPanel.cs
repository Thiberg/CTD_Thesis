using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

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

    [Header("Buttons")]
    public Button upgradeButton;
    public Button sellButton;

    private DragableItem currentNode;
    private PlacementSpot currentSpot;
    private RectTransform panelRect;

    private void Start()
    {
        panelRect = panelRoot.GetComponent<RectTransform>();
        panelRoot.SetActive(false);
        sellButton.onClick.AddListener(OnSellClicked);
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
    }

    private void Update()
    {
        if (!panelRoot.activeSelf) return;
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
            nodeLevel.text       = "Tier: " + node.nodeData.tier;
            nodeDescription.text = node.nodeData.description;
            ShowIconForType(node.nodeData.nodeType);
        }
        else
        {
            nodeName.text        = node.gameObject.name;
            nodeLevel.text       = "";
            nodeDescription.text = "";
        }

        panelRoot.SetActive(true);
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
            int refund = Mathf.RoundToInt(currentNode.nodeData.cost * 0.6f);
            GameManager.Instance?.AddBalance(refund);
        }

        GameManager.Instance?.UnregisterNode(currentNode);
        Destroy(currentNode.gameObject);
        Hide();
    }

    private void OnUpgradeClicked()
    {
        Debug.Log("Upgrade clicked for: " + (currentNode != null ? currentNode.nodeData.nodeName : "none"));
    }
}