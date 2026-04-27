using UnityEngine;
using UnityEngine.UI;
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

    private void Start()
    {
        panelRoot.SetActive(false);
        sellButton.onClick.AddListener(OnSellClicked);
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
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
            ShowIconForType(node.nodeData.nodeName);
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

    private void ShowIconForType(string name)
    {
        switch (name)
        {
            case "Service":  if (iconService)  iconService.SetActive(true);  break;
            case "Server":   if (iconServer)   iconServer.SetActive(true);   break;
            case "Database": if (iconDatabase) iconDatabase.SetActive(true); break;
            case "Firewall": if (iconFirewall) iconFirewall.SetActive(true); break;
            default:
                if (iconService) iconService.SetActive(true);
                break;
        }
    }

    private void OnSellClicked()
    {
        if (currentNode == null) return;
        Destroy(currentNode.gameObject);
        Hide();
    }

    private void OnUpgradeClicked()
    {
        Debug.Log("Upgrade clicked for: " + (currentNode != null ? currentNode.nodeData.nodeName : "none"));
    }
}