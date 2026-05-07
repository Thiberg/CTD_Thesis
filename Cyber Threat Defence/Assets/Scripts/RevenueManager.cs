using UnityEngine;

public class RevenueManager : MonoBehaviour
{
    private float tick;

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
            GameManager.Instance.AddBalance(node.nodeData.revenuePerSecond);
        }
    }

    private void ProcessUpkeep()
    {
        if (ConnectionManager.Instance == null) return;
        foreach (var node in GameManager.Instance.PlacedNodes)
        {
            if (node == null) continue;
            if (node.nodeData == null) continue;
            if (node.nodeData.nodeType != NodeType.Firewall) continue;
            if (node.nodeData.upkeepPerSecond <= 0f) continue;

            int connections = ConnectionManager.Instance.GetConnectionCount(node);
            if (connections == 0) continue;
            GameManager.Instance.DeductBalance(node.nodeData.upkeepPerSecond * connections);
        }
    }
}
