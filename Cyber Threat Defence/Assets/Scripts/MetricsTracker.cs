using System.Collections.Generic;
using UnityEngine;

// Computes the seven weighted end-of-game metrics on demand from current game state.
// Each metric outputs 0..1; total is a weighted average. Pure query — no event subscriptions,
// no per-frame work — called by EndScreenController and FirebaseUploader on game end.
public class MetricsTracker : MonoBehaviour
{
    public static MetricsTracker Instance { get; private set; }

    [Header("Benchmarks (raw value that scores 1.0)")]
    [Tooltip("Avg revenue rate ($/sec) that scores full marks. Below this scales linearly to 0.")]
    public float revenueRateBenchmark = 20f;
    [Tooltip("Final company value ($) that scores full marks. Below this scales linearly to 0.")]
    public float companyValueBenchmark = 5000f;
    [Tooltip("Avg extra connections per firewall that scores 0.0. 0 extras scores 1.0.")]
    public float firewallLoadCeiling = 5f;

    [Header("Credential Metric Coefficients")]
    [Tooltip("Bonus weight on rotations from shared → unique (defensive action).")]
    public float credRotationBonus = 2f;
    [Tooltip("Penalty per node still in a shared credential group at game end.")]
    public float credSharedAtEndPenalty = 1f;
    [Tooltip("Penalty per node compromised by leak DOT.")]
    public float credNodeLostPenalty = 1f;
    [Tooltip("Penalty per HP of leak DOT damage taken (default 0.01 = 100 HP equals 1 node lost).")]
    public float credDamagePerHpPenalty = 0.01f;

    [Header("Weights (should sum to 1.0)")]
    [Range(0f, 1f)] public float weightCredentialUniqueness = 0.25f;
    [Range(0f, 1f)] public float weightCriticalProtection   = 0.20f;
    [Range(0f, 1f)] public float weightFirewallLoad         = 0.15f;
    [Range(0f, 1f)] public float weightRevenue              = 0.15f;
    [Range(0f, 1f)] public float weightReputation           = 0.15f;
    [Range(0f, 1f)] public float weightPhishing             = 0.05f;
    [Range(0f, 1f)] public float weightCompanyValue         = 0.05f;

    public class FinalScore
    {
        public float credentialUniqueness;
        public float criticalProtection;
        public float firewallLoad;
        public float revenueRate;
        public float reputation;
        public float phishingAccuracy;
        public float companyValue;
        public float totalScore;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }


    public FinalScore Compute()
    {
        FinalScore s = new()
        {
            credentialUniqueness = ComputeCredentialUniqueness(),
            criticalProtection   = ComputeCriticalProtection(),
            firewallLoad         = ComputeFirewallLoad(),
            revenueRate          = ComputeRevenueRate(),
            reputation           = ComputeReputation(),
            phishingAccuracy     = ComputePhishingAccuracy(),
            companyValue         = ComputeCompanyValue(),
        };

        s.totalScore =
            s.credentialUniqueness * weightCredentialUniqueness +
            s.criticalProtection   * weightCriticalProtection +
            s.firewallLoad         * weightFirewallLoad +
            s.revenueRate          * weightRevenue +
            s.reputation           * weightReputation +
            s.phishingAccuracy     * weightPhishing +
            s.companyValue         * weightCompanyValue;

        return s;
    }

    // ── Per-metric computations ──────────────────────────────────────────────

    private float ComputeCredentialUniqueness()
    {
        if (CredentialManager.Instance == null) return 0.5f;
        int T = CredentialManager.Instance.TotalActivations;
        if (T == 0) return 0.5f; // No credential decisions made — neutral

        int U = CredentialManager.Instance.PlacementUniques;
        int R = CredentialManager.Instance.SharedToUniqueRotations;
        int S = CredentialManager.Instance.CountSharedNodes();
        int L = CredentialLeakManager.Instance != null ? CredentialLeakManager.Instance.NodesLostToLeaks : 0;
        int D = CredentialLeakManager.Instance != null ? CredentialLeakManager.Instance.TotalLeakDamageDealt : 0;

        float balance = U + credRotationBonus * R
                      - credSharedAtEndPenalty * S
                      - credNodeLostPenalty * L
                      - credDamagePerHpPenalty * D;

        return Mathf.Clamp01(0.5f + balance / (2f * T));
    }

    private float ComputeCriticalProtection()
    {
        if (GameManager.Instance == null || ConnectionManager.Instance == null) return 0f;
        int totalCritical = 0;
        int protectedCritical = 0;
        foreach (DragableItem n in GameManager.Instance.PlacedNodes)
        {
            if (n == null || n.nodeData == null) continue;
            if (n.nodeData.nodeType != NodeType.Service && n.nodeData.nodeType != NodeType.Database) continue;
            totalCritical++;

            List<DragableItem> firewalls = ConnectionManager.Instance.GetConnectedNodes(n, NodeType.Firewall);
            foreach (DragableItem fw in firewalls)
            {
                if (fw != null && !fw.IsCompromised)
                {
                    protectedCritical++;
                    break;
                }
            }
        }
        return totalCritical == 0 ? 0f : (float)protectedCritical / totalCritical;
    }

    private float ComputeFirewallLoad()
    {
        if (GameManager.Instance == null || ConnectionManager.Instance == null) return 0f;
        int firewallCount = 0;
        int totalExtra = 0;
        foreach (DragableItem n in GameManager.Instance.PlacedNodes)
        {
            if (n == null || n.nodeData == null) continue;
            if (n.nodeData.nodeType != NodeType.Firewall) continue;
            firewallCount++;
            int conns = ConnectionManager.Instance.GetConnectionCount(n);
            totalExtra += Mathf.Max(0, conns - 1);
        }
        if (firewallCount == 0) return 0f;
        float avgExtra = (float)totalExtra / firewallCount;
        return Mathf.Clamp01(1f - avgExtra / firewallLoadCeiling);
    }

    private float ComputeRevenueRate()
    {
        if (RevenueManager.Instance == null || GameManager.Instance == null) return 0f;
        float elapsed = GameManager.Instance.sessionDuration - GameManager.Instance.TimeRemaining;
        if (elapsed <= 0f) return 0f;
        float rate = RevenueManager.Instance.TotalRevenueEarned / elapsed;
        return Mathf.Clamp01(rate / revenueRateBenchmark);
    }

    private float ComputeReputation()
    {
        if (GameManager.Instance == null) return 0f;
        return Mathf.Clamp01(GameManager.Instance.Reputation / 100f);
    }

    private float ComputePhishingAccuracy()
    {
        if (MessageManager.Instance == null) return 0f;
        int correct = MessageManager.Instance.PhishingCorrectCount;
        int incorrect = MessageManager.Instance.PhishingIncorrectCount;

        int unanswered = 0;
        foreach (MessageData msg in MessageManager.Instance.Inbox)
            if (msg != null && msg.type == MessageType.Phishing) unanswered++;

        int total = correct + incorrect + unanswered;
        if (total == 0) return 0f;
        return (float)correct / total;
    }

    private float ComputeCompanyValue()
    {
        if (GameManager.Instance == null) return 0f;
        float value = GameManager.Instance.Balance;
        foreach (DragableItem n in GameManager.Instance.PlacedNodes)
        {
            if (n == null) continue;
            value += n.GetSellRefund();
        }
        return Mathf.Clamp01(value / companyValueBenchmark);
    }
}
