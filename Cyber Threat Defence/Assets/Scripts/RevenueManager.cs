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
            // TODO Step 7: replace with IsChainIntact(node) once connections are built
            GameManager.Instance.AddBalance(node.nodeData.revenuePerSecond);
        }
    }
}
