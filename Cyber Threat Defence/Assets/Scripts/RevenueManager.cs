using UnityEngine;

public class RevenueManager : MonoBehaviour
{
    public static RevenueManager Instance { get; private set; }
    public float TotalRevenueEarned { get; private set; }

    private float tick;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.GameActive) return;

        tick += Time.deltaTime;
        if (tick >= 1f)
        {
            tick -= 1f;
            ProcessRevenue();
            ProcessUpkeep();
        }
    }

    private void ProcessRevenue()
    {
        foreach (var node in GameManager.Instance.PlacedNodes)
        {
            if (node == null) continue;
            if (node.nodeData == null) continue;
            if (node.nodeData.nodeType != NodeType.Service) continue;
            if (node.IsCompromised) continue;
            if (ConnectionManager.Instance != null && !ConnectionManager.Instance.IsChainIntact(node)) continue;
            GameManager.Instance.AddBalance(node.CurrentRevenuePerSecond);
            TotalRevenueEarned += node.CurrentRevenuePerSecond;
        }
    }

    private void ProcessUpkeep()
    {
        foreach (var node in GameManager.Instance.PlacedNodes)
        {
            if (node == null) continue;
            if (node.nodeData == null) continue;
            if (node.IsCompromised) continue;
            if (node.CurrentUpkeepPerSecond <= 0f) continue;

            if (node.nodeData.nodeType == NodeType.Firewall)
            {
                if (ConnectionManager.Instance == null) continue;
                int connections = ConnectionManager.Instance.GetConnectionCount(node);
                if (connections == 0) continue;
                GameManager.Instance.DeductBalance(node.CurrentUpkeepPerSecond * connections);
            }
            else if (node.nodeData.nodeType == NodeType.Server || node.nodeData.nodeType == NodeType.Database)
            {
                GameManager.Instance.DeductBalance(node.CurrentUpkeepPerSecond);
            }
        }
    }
}
