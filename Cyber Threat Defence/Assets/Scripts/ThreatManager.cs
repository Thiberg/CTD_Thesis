using System.Collections.Generic;
using UnityEngine;

public class ThreatManager : MonoBehaviour
{
    public static ThreatManager Instance { get; private set; }

    [Header("Attack Scheduling")]
    public float minAttackInterval = 15f;
    public float maxAttackInterval = 25f;

    [Header("Attack Timer (Stage 1 defaults)")]
    public float minAttackDuration = 20f;
    public float maxAttackDuration = 30f;

    [Header("Damage")]
    public int damagePerBreach = 40;

    [Header("Target Weights")]
    public int weightService  = 40;
    public int weightDatabase = 40;
    public int weightFirewall = 0;   // Firewalls only attacked via redirection, never directly picked
    public int weightServer   = 20;

    [Header("Breach Probability")]
    [Range(0f, 1f)] public float minBreachChance = 0.05f;
    [Range(0f, 1f)] public float firewallReduction = 0.25f;
    // Each connection beyond the first erodes a firewall's protection by this much. Capped at zero net reduction.
    [Range(0f, 1f)] public float firewallLoadPenalty = 0.05f;

    private float nextAttackTime;
    private readonly List<ActiveAttack> activeAttacks = new();

    private class ActiveAttack
    {
        public DragableItem target;
        public float timeRemaining;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        ScheduleNextAttack();
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.GameActive) return;

        for (int i = activeAttacks.Count - 1; i >= 0; i--)
        {
            ActiveAttack atk = activeAttacks[i];
            if (atk.target == null)
            {
                activeAttacks.RemoveAt(i);
                continue;
            }

            atk.timeRemaining -= Time.deltaTime;
            if (atk.timeRemaining <= 0f)
            {
                ResolveAttack(atk);
                activeAttacks.RemoveAt(i);
            }
        }

        if (Time.time >= nextAttackTime)
        {
            TrySpawnAttack();
            ScheduleNextAttack();
        }
    }

    private void ScheduleNextAttack()
    {
        nextAttackTime = Time.time + Random.Range(minAttackInterval, maxAttackInterval);
    }

    private void TrySpawnAttack()
    {
        DragableItem target = PickTarget();
        if (target == null) return;

        target.SetUnderAttack(true);
        float duration = Random.Range(minAttackDuration, maxAttackDuration);
        activeAttacks.Add(new ActiveAttack { target = target, timeRemaining = duration });
    }

    private DragableItem PickTarget()
    {
        if (GameManager.Instance == null) return null;

        List<DragableItem> eligible = new();
        foreach (DragableItem node in GameManager.Instance.PlacedNodes)
        {
            if (node == null) continue;
            if (node.nodeData == null) continue;
            if (!node.hasBeenPlaced) continue;
            if (node.IsUnderAttack) continue;
            if (node.IsCompromised) continue;
            eligible.Add(node);
        }
        if (eligible.Count == 0) return null;

        int totalWeight = 0;
        foreach (DragableItem node in eligible)
            totalWeight += GetWeight(node.nodeData.nodeType);
        if (totalWeight == 0) return null;

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (DragableItem node in eligible)
        {
            cumulative += GetWeight(node.nodeData.nodeType);
            if (roll < cumulative) return node;
        }
        return eligible[eligible.Count - 1];
    }

    private int GetWeight(NodeType type) => type switch
    {
        NodeType.Service  => weightService,
        NodeType.Database => weightDatabase,
        NodeType.Firewall => weightFirewall,
        NodeType.Server   => weightServer,
        _                 => 0,
    };

    private void ResolveAttack(ActiveAttack atk)
    {
        if (atk.target == null) return;
        atk.target.SetUnderAttack(false);

        if (atk.target.nodeData == null) return;

        float chance = ComputeBreachChance(atk.target);
        bool breached = Random.value < chance;

        if (!breached) return;

        DragableItem recipient = PickDamageRecipient(atk.target);
        recipient.ApplyDamage(damagePerBreach);
    }

    private DragableItem PickDamageRecipient(DragableItem target)
    {
        if (ConnectionManager.Instance == null) return target;

        List<DragableItem> firewalls = ConnectionManager.Instance.GetConnectedNodes(target, NodeType.Firewall);
        List<DragableItem> healthy = new();
        foreach (DragableItem fw in firewalls)
        {
            if (fw == null) continue;
            if (fw.IsCompromised) continue;
            healthy.Add(fw);
        }
        if (healthy.Count == 0) return target;
        return healthy[Random.Range(0, healthy.Count)];
    }

    private float ComputeBreachChance(DragableItem target)
    {
        if (target.nodeData == null) return 0f;

        float chance = (6 - target.nodeData.securityLevel) * 0.15f;

        if (ConnectionManager.Instance != null)
        {
            List<DragableItem> firewalls = ConnectionManager.Instance.GetConnectedNodes(target, NodeType.Firewall);
            foreach (DragableItem fw in firewalls)
            {
                if (fw == null) continue;
                if (fw.IsCompromised) continue;

                // Wider firewalls protect less per node. Floored at 0 — overloaded firewalls offer no protection but never harm.
                int extraConnections = Mathf.Max(0, ConnectionManager.Instance.GetConnectionCount(fw) - 1);
                float netReduction = Mathf.Max(0f, firewallReduction - firewallLoadPenalty * extraConnections);
                chance -= netReduction;
            }
        }

        return Mathf.Clamp(chance, minBreachChance, 1f);
    }
}
