using System.Collections.Generic;
using UnityEngine;

public class ThreatManager : MonoBehaviour
{
    [System.Serializable]
    public class StageConfig
    {
        public float minAttackInterval = 15f;
        public float maxAttackInterval = 25f;
        public float minAttackDuration = 20f;
        public float maxAttackDuration = 30f;
        [Range(0f, 1f)] public float minBreachChance = 0.05f;
        public int weightService  = 40;
        public int weightDatabase = 40;
        public int weightServer   = 20;
        public int weightFirewall = 0;

        public static StageConfig Stage2Default() => new()
        {
            minAttackInterval = 10f, maxAttackInterval = 18f,
            minAttackDuration = 15f, maxAttackDuration = 25f,
            minBreachChance = 0.10f,
            weightService = 30, weightDatabase = 50, weightServer = 20, weightFirewall = 0,
        };

        public static StageConfig Stage3Default() => new()
        {
            minAttackInterval = 7f, maxAttackInterval = 13f,
            minAttackDuration = 10f, maxAttackDuration = 18f,
            minBreachChance = 0.18f,
            weightService = 20, weightDatabase = 60, weightServer = 20, weightFirewall = 0,
        };
    }

    public static ThreatManager Instance { get; private set; }

    // Stage-controlled runtime values — tune via Stage Configs below, not these.
    [HideInInspector] public float minAttackInterval = 15f;
    [HideInInspector] public float maxAttackInterval = 25f;
    [HideInInspector] public float minAttackDuration = 20f;
    [HideInInspector] public float maxAttackDuration = 30f;
    [HideInInspector] public int weightService  = 40;
    [HideInInspector] public int weightDatabase = 40;
    [HideInInspector] public int weightFirewall = 0;
    [HideInInspector] public int weightServer   = 20;
    [HideInInspector, Range(0f, 1f)] public float minBreachChance = 0.05f;

    [Header("Damage")]
    public int damagePerBreach = 40;

    [Header("Grace Period")]
    [Tooltip("Seconds at game start during which no attacks fire.")]
    public float initialGracePeriod = 10f;

    [Header("Firewall")]
    [Range(0f, 1f)] public float firewallReduction = 0.25f;
    // Each connection beyond the first erodes a firewall's protection by this much. Capped at zero net reduction.
    [Range(0f, 1f)] public float firewallLoadPenalty = 0.05f;

    [Header("Stage Configs")]
    public StageConfig stage1 = new();
    public StageConfig stage2 = StageConfig.Stage2Default();
    public StageConfig stage3 = StageConfig.Stage3Default();

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

        stage1 ??= new StageConfig();
        stage2 ??= StageConfig.Stage2Default();
        stage3 ??= StageConfig.Stage3Default();
    }

    private void OnEnable()
    {
        StageManager.OnStageChanged += ApplyStage;
        ApplyStage(StageManager.Instance != null ? StageManager.Instance.CurrentStage : 1);
    }

    private void OnDisable()
    {
        StageManager.OnStageChanged -= ApplyStage;
    }

    private void Start()
    {
        // First attack waits the full grace period plus a normal interval.
        nextAttackTime = Time.time + initialGracePeriod + Random.Range(minAttackInterval, maxAttackInterval);
        OnboardingController.OnOnboardingCompleted += RescheduleFromNow;
    }

    private void OnDestroy()
    {
        OnboardingController.OnOnboardingCompleted -= RescheduleFromNow;
    }

    private void RescheduleFromNow()
    {
        nextAttackTime = Time.time + initialGracePeriod + Random.Range(minAttackInterval, maxAttackInterval);
    }

    private void ApplyStage(int stage)
    {
        StageConfig cfg = stage switch
        {
            2 => stage2,
            3 => stage3,
            _ => stage1,
        };
        if (cfg == null) return;

        minAttackInterval = cfg.minAttackInterval;
        maxAttackInterval = cfg.maxAttackInterval;
        minAttackDuration = cfg.minAttackDuration;
        maxAttackDuration = cfg.maxAttackDuration;
        minBreachChance   = cfg.minBreachChance;
        weightService  = cfg.weightService;
        weightDatabase = cfg.weightDatabase;
        weightServer   = cfg.weightServer;
        weightFirewall = cfg.weightFirewall;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.GameActive) return;
        if (OnboardingController.Instance != null && OnboardingController.Instance.IsActive) return;

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

        float chance = (6 - target.CurrentSecurityLevel) * 0.15f;

        if (ConnectionManager.Instance != null)
        {
            List<DragableItem> firewalls = ConnectionManager.Instance.GetConnectedNodes(target, NodeType.Firewall);
            foreach (DragableItem fw in firewalls)
            {
                if (fw == null) continue;
                if (fw.IsCompromised) continue;

                // Wider firewalls protect less per node. Floored at 0 — overloaded firewalls offer no protection but never harm.
                int extraConnections = Mathf.Max(0, ConnectionManager.Instance.GetConnectionCount(fw) - 1);
                float reduction = firewallReduction + fw.CurrentFirewallReductionBonus;
                float netReduction = Mathf.Max(0f, reduction - firewallLoadPenalty * extraConnections);
                chance -= netReduction;
            }
        }

        return Mathf.Clamp(chance, minBreachChance, 1f);
    }
}
