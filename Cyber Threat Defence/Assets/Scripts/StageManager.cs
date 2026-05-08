using System;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("Stage Thresholds (Time Remaining, seconds)")]
    [Tooltip("Time remaining when Stage 2 begins. Default 600 = 10 min left in a 15-min session.")]
    public float stage2StartsAt = 600f;
    [Tooltip("Time remaining when Stage 3 begins. Default 300 = 5 min left.")]
    public float stage3StartsAt = 300f;

    public int CurrentStage { get; private set; } = 1;

    public static event Action<int> OnStageChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.GameActive) return;

        float t = GameManager.Instance.TimeRemaining;
        int newStage;
        if (t <= stage3StartsAt)      newStage = 3;
        else if (t <= stage2StartsAt) newStage = 2;
        else                          newStage = 1;

        if (newStage != CurrentStage)
        {
            CurrentStage = newStage;
            Debug.Log($"[Stage] Transitioned to Stage {newStage}");
            OnStageChanged?.Invoke(CurrentStage);
        }
    }
}
