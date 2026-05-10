using System;
using System.Collections.Generic;
using UnityEngine;

// Central game state. Owns balance, reputation, the session timer, the placed-node registry,
// the win/lose conditions, and the static events the HUD subscribes to. Pauses during
// onboarding and before the first node is placed.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Starting Values")]
    public float startingBalance = 1000f;
    public float startingReputation = 100f;
    public float sessionDuration = 900f; // 15 minutes

    [Header("Penalties")]
    public float reputationLossOnCompromise = 15f;

    [Header("Lose Conditions")]
    [Tooltip("Game over if no service earns revenue for this many seconds (only counts once the player has earned at least once).")]
    public float noRevenueLoseThreshold = 60f;

    private float noRevenueTimer;
    private bool hasEverEarned;
    private bool firstNodePlaced;

    public float Balance { get; private set; }
    public float Reputation { get; private set; }
    public float TimeRemaining { get; private set; }
    public bool GameActive { get; private set; }

    public static event Action<float> OnBalanceChanged;
    public static event Action<float> OnReputationChanged;
    public static event Action<float> OnTimerChanged;
    public static event Action<string> OnGameOver;
    public static event Action OnGameWon;

    public List<DragableItem> PlacedNodes { get; } = new List<DragableItem>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        SessionIdentity.StartNewSession();
    }

    private void Start()
    {
        Balance = startingBalance;
        Reputation = startingReputation;
        TimeRemaining = sessionDuration;
        GameActive = true;

        OnBalanceChanged?.Invoke(Balance);
        OnReputationChanged?.Invoke(Reputation);
        OnTimerChanged?.Invoke(TimeRemaining);
    }

    private void Update()
    {
        if (!GameActive) return;
        if (OnboardingController.Instance != null && OnboardingController.Instance.IsActive) return;
        if (!firstNodePlaced) return;

        TimeRemaining -= Time.deltaTime;
        OnTimerChanged?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            TriggerWin();
            return;
        }

        CheckLoseConditions();
    }

    public bool CanAfford(int cost) => Balance >= cost;

    public void AddBalance(float amount)
    {
        Balance += amount;
        OnBalanceChanged?.Invoke(Balance);
    }

    public void DeductBalance(float amount)
    {
        Balance -= amount;
        OnBalanceChanged?.Invoke(Balance);
    }

    public void ModifyReputation(float delta)
    {
        Reputation = Mathf.Clamp(Reputation + delta, 0f, 100f);
        OnReputationChanged?.Invoke(Reputation);
    }

    public void RegisterNode(DragableItem node)
    {
        if (!PlacedNodes.Contains(node))
        {
            PlacedNodes.Add(node);
            firstNodePlaced = true;
        }
    }

    public void UnregisterNode(DragableItem node)
    {
        PlacedNodes.Remove(node);
    }

    private void CheckLoseConditions()
    {
        if (Reputation <= 0f)
        {
            TriggerGameOver("Your company's reputation collapsed after repeated uncontained breaches.");
            return;
        }

        // No-revenue lose timer: only counts after the player has earned at least once,
        // so the early-game setup phase isn't punished.
        bool earning = HasAnyEarningService();
        if (earning)
        {
            hasEverEarned = true;
            noRevenueTimer = 0f;
        }
        else if (hasEverEarned)
        {
            noRevenueTimer += Time.deltaTime;
            if (noRevenueTimer >= noRevenueLoseThreshold)
                TriggerGameOver("Operations stalled — no revenue generated for over a minute.");
        }
    }

    private bool HasAnyEarningService()
    {
        foreach (var node in PlacedNodes)
        {
            if (node == null) continue;
            if (node.nodeData == null) continue;
            if (node.nodeData.nodeType != NodeType.Service) continue;
            if (node.IsCompromised) continue;
            if (ConnectionManager.Instance == null) continue;
            if (!ConnectionManager.Instance.IsChainIntact(node)) continue;
            return true;
        }
        return false;
    }

    private void TriggerGameOver(string reason)
    {
        GameActive = false;
        Debug.Log("GAME OVER: " + reason);
        OnGameOver?.Invoke(reason);
    }

    private void TriggerWin()
    {
        GameActive = false;
        Debug.Log("GAME WON");
        OnGameWon?.Invoke();
    }
}
