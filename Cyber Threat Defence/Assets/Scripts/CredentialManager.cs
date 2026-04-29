using System.Collections.Generic;
using UnityEngine;

public class CredentialManager : MonoBehaviour
{
    public static CredentialManager Instance { get; private set; }

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
        Debug.Log($"{node.nodeData.nodeName} → new credential group {groupId}");
    }

    public void AssignLastCredential(DragableItem node)
    {
        // Should only be called when HasLastCredential is true
        nodeCredentials[node] = lastUsedGroupId;
        Debug.Log($"{node.nodeData.nodeName} → shared credential group {lastUsedGroupId}");
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
}
