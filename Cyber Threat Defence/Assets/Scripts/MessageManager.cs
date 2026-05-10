using System;
using System.Collections.Generic;
using UnityEngine;

// Spawns mails on a randomised interval, tracks correct/incorrect responses (separately
// counts phishing-only for the metric), and filters the pool by current stage so phishing
// difficulty escalates over time. Wrong answers cost reputation.
public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance { get; private set; }

    [Header("Pool")]
    public List<MessageData> messagePool = new();

    [Header("Spawn Timing")]
    public float minSpawnInterval = 45f;
    public float maxSpawnInterval = 90f;

    [Header("Penalties")]
    public float reputationLossOnIncorrect = 5f;

    private float nextSpawnTime;

    private readonly List<MessageData> inbox = new();
    public IReadOnlyList<MessageData> Inbox => inbox;
    public int UnreadCount => inbox.Count;

    public int CorrectCount { get; private set; }
    public int IncorrectCount { get; private set; }
    public int PhishingCorrectCount { get; private set; }
    public int PhishingIncorrectCount { get; private set; }

    public static event Action OnInboxChanged;
    public static event Action<MessageData, MessageAction, bool> OnMessageResolved;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        ScheduleNextSpawn();
        OnboardingController.OnOnboardingCompleted += ScheduleNextSpawn;
    }

    private void OnDestroy()
    {
        OnboardingController.OnOnboardingCompleted -= ScheduleNextSpawn;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.GameActive) return;
        if (OnboardingController.Instance != null && OnboardingController.Instance.IsActive) return;
        if (messagePool.Count == 0) return;
        if (Time.time < nextSpawnTime) return;

        SpawnMessage();
        ScheduleNextSpawn();
    }

    private void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + UnityEngine.Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    private void SpawnMessage()
    {
        MessageData msg = PickFromPool();
        if (msg == null)
        {
            Debug.LogWarning("[MessageManager] Pool has no valid MessageData. Check Message Pool in Inspector for empty slots.");
            return;
        }

        inbox.Insert(0, msg); // Newest on top
        OnInboxChanged?.Invoke();
    }

    private MessageData PickFromPool()
    {
        int validCount = 0;
        for (int i = 0; i < messagePool.Count; i++)
            if (IsEligible(messagePool[i])) validCount++;

        if (validCount == 0) return null;

        int target = UnityEngine.Random.Range(0, validCount);
        int seen = 0;
        for (int i = 0; i < messagePool.Count; i++)
        {
            if (!IsEligible(messagePool[i])) continue;
            if (seen == target) return messagePool[i];
            seen++;
        }
        return null;
    }

    private static bool IsEligible(MessageData msg)
    {
        if (msg == null) return false;
        if (msg.stage == 0) return true;
        int current = StageManager.Instance != null ? StageManager.Instance.CurrentStage : 1;
        return msg.stage <= current;
    }

    public void RespondTo(MessageData msg, MessageAction action)
    {
        if (msg == null) return;
        if (!inbox.Remove(msg)) return;

        bool correct = IsCorrectAction(msg.type, action);
        if (correct) CorrectCount++; else IncorrectCount++;

        if (msg.type == MessageType.Phishing)
        {
            if (correct) PhishingCorrectCount++; else PhishingIncorrectCount++;
        }

        if (!correct && GameManager.Instance != null)
            GameManager.Instance.ModifyReputation(-reputationLossOnIncorrect);

        OnInboxChanged?.Invoke();
        OnMessageResolved?.Invoke(msg, action, correct);
    }

    public static bool IsCorrectAction(MessageType type, MessageAction action) => type switch
    {
        MessageType.Normal   => action == MessageAction.Reply,
        MessageType.Phishing => action == MessageAction.Report,
        _ => false,
    };
}
