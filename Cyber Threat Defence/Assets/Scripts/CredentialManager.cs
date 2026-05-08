using System.Collections.Generic;
using UnityEngine;

public class CredentialManager : MonoBehaviour
{
    public static CredentialManager Instance { get; private set; }

    // Lifetime counters used by MetricsTracker for the credential uniqueness score.
    public int PlacementUniques { get; private set; }
    public int MidGameRotations { get; private set; }
    public int SharedToUniqueRotations { get; private set; }
    public int TotalActivations { get; private set; }

    private int nextGroupId = 1;
    private int lastUsedGroupId = -1; // -1 = no credential assigned yet
    private readonly Dictionary<DragableItem, int> nodeCredentials = new();

    public bool HasLastCredential => lastUsedGroupId >= 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AssignNewCredential(DragableItem node)
    {
        GameManager.Instance?.DeductBalance(50f);
        int groupId = nextGroupId++;
        nodeCredentials[node] = groupId;
        lastUsedGroupId = groupId;
        PlacementUniques++;
        TotalActivations++;
    }

    public void AssignLastCredential(DragableItem node)
    {
        // Should only be called when HasLastCredential is true
        nodeCredentials[node] = lastUsedGroupId;
        TotalActivations++;
    }

    // Mid-game password change. Caller (NodeInspectorPanel) handles charging the player.
    // Does NOT update lastUsedGroupId — a defensive swap shouldn't bleed into the next placement.
    public void SwapToNewCredential(DragableItem node)
    {
        if (node == null) return;
        int oldGroupSize = CountInGroup(GetGroup(node));
        nodeCredentials[node] = nextGroupId++;
        MidGameRotations++;
        if (oldGroupSize > 1) SharedToUniqueRotations++;
    }

    // Called when a node is destroyed or sold
    public void UnregisterNode(DragableItem node)
    {
        nodeCredentials.Remove(node);
    }

    public int GetGroup(DragableItem node) =>
        nodeCredentials.TryGetValue(node, out int g) ? g : -1;

    // Returns all nodes that share the same credential group as the given node
    public List<DragableItem> GetGroupMembers(DragableItem node)
    {
        if (!nodeCredentials.TryGetValue(node, out int groupId)) return new List<DragableItem>();

        var result = new List<DragableItem>();
        foreach (var kvp in nodeCredentials)
        {
            if (kvp.Value == groupId && kvp.Key != node)
                result.Add(kvp.Key);
        }
        return result;
    }

    // Count of nodes whose credential group still has at least one other member.
    public int CountSharedNodes()
    {
        int count = 0;
        foreach (var kvp in nodeCredentials)
        {
            int groupId = kvp.Value;
            foreach (var other in nodeCredentials)
            {
                if (other.Key != kvp.Key && other.Value == groupId) { count++; break; }
            }
        }
        return count;
    }

    private int CountInGroup(int groupId)
    {
        if (groupId < 0) return 0;
        int count = 0;
        foreach (var v in nodeCredentials.Values)
            if (v == groupId) count++;
        return count;
    }
}
