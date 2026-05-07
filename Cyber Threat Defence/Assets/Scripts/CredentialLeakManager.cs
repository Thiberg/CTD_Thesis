using System.Collections.Generic;
using UnityEngine;

public class CredentialLeakManager : MonoBehaviour
{
    public static CredentialLeakManager Instance { get; private set; }

    [Header("DOT")]
    public int damagePerSecond = 5;
    public float tickInterval = 1f;

    private float tickTimer;
    private readonly HashSet<DragableItem> leakingNodes = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.GameActive) return;

        tickTimer += Time.deltaTime;
        if (tickTimer < tickInterval) return;
        tickTimer -= tickInterval;

        // Snapshot — ApplyDamage may compromise a node, which triggers further StartLeak calls
        // that mutate the set. Iterating a copy keeps us safe.
        List<DragableItem> snapshot = new(leakingNodes);
        foreach (DragableItem node in snapshot)
        {
            if (node == null) { leakingNodes.Remove(node); continue; }
            if (node.IsCompromised) { StopLeak(node); continue; }

            node.ApplyDamage(damagePerSecond);
            if (node.IsCompromised) StopLeak(node);
        }
    }

    // Called when `source` has just been compromised — leak through every healthy member of its credential group.
    public void StartLeakOnGroupOf(DragableItem source)
    {
        if (source == null || CredentialManager.Instance == null) return;

        List<DragableItem> members = CredentialManager.Instance.GetGroupMembers(source);
        foreach (DragableItem member in members)
            StartLeak(member);
    }

    public void StartLeak(DragableItem node)
    {
        if (node == null) return;
        if (node.IsCompromised) return;
        if (!leakingNodes.Add(node)) return;

        node.SetLeaking(true);
    }

    public void StopLeak(DragableItem node)
    {
        if (node == null) return;
        if (!leakingNodes.Remove(node)) return;
        node.SetLeaking(false);
    }

    public bool IsLeaking(DragableItem node) => node != null && leakingNodes.Contains(node);
}
