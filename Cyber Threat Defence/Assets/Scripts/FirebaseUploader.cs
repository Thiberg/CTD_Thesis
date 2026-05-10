using System;
using System.Collections;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// Uploads end-of-game metrics + outcome to Firebase Realtime Database via REST API.
// Uses PUT with the player's session ID as the document key, so Firebase entries are
// keyed by the same short ID shown to the player on the questionnaire screen.
public class FirebaseUploader : MonoBehaviour
{
    [Header("Firebase Config")]
    [Tooltip("Realtime Database URL — e.g., https://your-project-default-rtdb.region.firebasedatabase.app/")]
    public string firebaseDatabaseUrl = "your-database-url";

    [Tooltip("Path within the database to write sessions to (auto-creates if missing).")]
    public string collectionName = "sessions";

    private void OnEnable()
    {
        GameManager.OnGameWon  += HandleWin;
        GameManager.OnGameOver += HandleLose;
    }

    private void OnDisable()
    {
        GameManager.OnGameWon  -= HandleWin;
        GameManager.OnGameOver -= HandleLose;
    }

    private void HandleWin()          => SendResult("win", null);
    private void HandleLose(string r) => SendResult("lose", r);

    private void SendResult(string outcome, string reason)
    {
        if (MetricsTracker.Instance == null) return;
        if (string.IsNullOrEmpty(firebaseDatabaseUrl) || firebaseDatabaseUrl == "your-database-url")
        {
            Debug.LogWarning("[Firebase] Database URL not set — skipping upload.");
            return;
        }
        StartCoroutine(PostToFirebase(MetricsTracker.Instance.Compute(), outcome, reason));
    }

    private IEnumerator PostToFirebase(MetricsTracker.FinalScore s, string outcome, string reason)
    {
        string sessionId = SessionIdentity.CurrentSessionId;
        string url  = $"{firebaseDatabaseUrl.TrimEnd('/')}/{collectionName}/{sessionId}.json";
        string body = BuildBody(s, outcome, reason);

        UnityWebRequest req = new(url, "PUT")
        {
            uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            Debug.Log($"[Firebase] Session {sessionId} uploaded ({outcome})");
        else
            Debug.LogError($"[Firebase] Upload failed: {req.error} | {req.downloadHandler.text}");
    }

    private static string BuildBody(MetricsTracker.FinalScore s, string outcome, string reason)
    {
        IFormatProvider ci = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        sb.Append("{");
        sb.Append($"\"sessionId\":\"{Escape(SessionIdentity.CurrentSessionId)}\",");
        sb.Append($"\"outcome\":\"{Escape(outcome)}\",");
        if (!string.IsNullOrEmpty(reason))
            sb.Append($"\"loseReason\":\"{Escape(reason)}\",");
        sb.Append($"\"timestamp\":\"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}\",");
        float timeLeft = GameManager.Instance != null ? GameManager.Instance.TimeRemaining : 0f;
        sb.Append($"\"timeRemainingSeconds\":{timeLeft.ToString("0.00", ci)},");
        sb.Append($"\"credentialUniqueness\":{s.credentialUniqueness.ToString("0.0000", ci)},");
        sb.Append($"\"criticalProtection\":{s.criticalProtection.ToString("0.0000", ci)},");
        sb.Append($"\"firewallLoad\":{s.firewallLoad.ToString("0.0000", ci)},");
        sb.Append($"\"revenueRate\":{s.revenueRate.ToString("0.0000", ci)},");
        sb.Append($"\"reputation\":{s.reputation.ToString("0.0000", ci)},");
        sb.Append($"\"phishingAccuracy\":{s.phishingAccuracy.ToString("0.0000", ci)},");
        sb.Append($"\"companyValue\":{s.companyValue.ToString("0.0000", ci)},");
        sb.Append($"\"totalScore\":{s.totalScore.ToString("0.0000", ci)}");
        sb.Append("}");
        return sb.ToString();
    }

    private static string Escape(string s)
        => string.IsNullOrEmpty(s) ? "" : s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
