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
    [Range(0f, 1f)] public float damagePercentPerBreach = 0.25f;

    [Header("Target Weights")]
    public int weightService  = 35;
    public int weightDatabase = 35;
    public int weightFirewall = 20;
    public int weightServer   = 10;

    [Header("Breach Probability")]
    [Range(0f, 1f)] public float minBreachChance = 0.05f;
    [Range(0f, 1f)] public float firewallReduction = 0.25f;

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

        Debug.Log($"[ThreatManager] Attack started on {target.nodeData?.nodeName} " +
                  $"(security {target.nodeData?.securityLevel}) — hidden timer {duration:0.0}s");
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

        if (!breached)
        {
            Debug.Log($"[ThreatManager] Attack on {atk.target.nodeData.nodeName} FAILED — breach chance was {chance:P0}");
            return;
        }

        int damage = Mathf.CeilToInt(atk.target.nodeData.maxHealth * damagePercentPerBreach);
        atk.target.ApplyDamage(damage);

        Debug.Log($"[ThreatManager] BREACH on {atk.target.nodeData.nodeName} — chance {chance:P0}, " +
                  $"dealt {damage} dmg, HP now {atk.target.CurrentHealth}/{atk.target.nodeData.maxHealth}" +
                  (atk.target.IsCompromised ? " [DESTROYED]" : ""));
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
                chance -= firewallReduction;
            }
        }

        return Mathf.Clamp(chance, minBreachChance, 1f);
    }
}
