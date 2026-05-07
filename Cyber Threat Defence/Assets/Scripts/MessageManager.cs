using System;
using System.Collections.Generic;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance { get; private set; }

    [Header("Pool")]
    public List<MessageData> messagePool = new();

    [Header("Spawn Timing")]
    public float minSpawnInterval = 45f;
    public float maxSpawnInterval = 90f;

    private float nextSpawnTime;

    private readonly List<MessageData> inbox = new();
    public IReadOnlyList<MessageData> Inbox => inbox;
    public int UnreadCount => inbox.Count;

    public int CorrectCount { get; private set; }
    public int IncorrectCount { get; private set; }

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
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.GameActive) return;
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
        Debug.Log($"[MessageManager] New message: \"{msg.subject}\" from {msg.sender} (type: {msg.type})");
        OnInboxChanged?.Invoke();
    }

    private MessageData PickFromPool()
    {
        int validCount = 0;
        for (int i = 0; i < messagePool.Count; i++)
            if (messagePool[i] != null) validCount++;

        if (validCount == 0) return null;

        int target = UnityEngine.Random.Range(0, validCount);
        int seen = 0;
        for (int i = 0; i < messagePool.Count; i++)
        {
            if (messagePool[i] == null) continue;
            if (seen == target) return messagePool[i];
            seen++;
        }
        return null;
    }

    public void RespondTo(MessageData msg, MessageAction action)
    {
        if (msg == null) return;
        if (!inbox.Remove(msg)) return;

        bool correct = IsCorrectAction(msg.type, action);
        if (correct) CorrectCount++; else IncorrectCount++;

        Debug.Log($"[MessageManager] {action} → \"{msg.subject}\" — {(correct ? "CORRECT" : "INCORRECT")} " +
                  $"(running: {CorrectCount} correct / {IncorrectCount} incorrect)");

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
