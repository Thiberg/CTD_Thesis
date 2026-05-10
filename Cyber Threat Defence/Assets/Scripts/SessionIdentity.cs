using System.Collections.Generic;
using UnityEngine;

public static class SessionIdentity
{
    private const string SAFE_ALPHABET = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no 0/O/1/I/l
    private const int ID_LENGTH = 8;

    // Static state: persists across scene reloads (in-game Restart button) but resets
    // when the Unity application terminates — i.e., browser refresh, tab close, Stop+Play.
    private static readonly List<string> sessionIds = new();
    private static string currentSessionId;

    public static string CurrentSessionId
    {
        get
        {
            if (string.IsNullOrEmpty(currentSessionId)) StartNewSession();
            return currentSessionId;
        }
    }

    // Forces a fresh ID for this playthrough and appends it to the session list.
    public static void StartNewSession()
    {
        currentSessionId = GenerateShortId();
        sessionIds.Add(currentSessionId);
    }

    public static List<string> GetAllSessionIds() => new(sessionIds);

    private static string GenerateShortId()
    {
        var sb = new System.Text.StringBuilder(ID_LENGTH);
        for (int i = 0; i < ID_LENGTH; i++)
            sb.Append(SAFE_ALPHABET[UnityEngine.Random.Range(0, SAFE_ALPHABET.Length)]);
        return sb.ToString();
    }
}
