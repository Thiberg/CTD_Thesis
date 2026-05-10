using UnityEngine;

public static class SessionIdentity
{
    private const string SESSION_ID_KEY = "novacorp_session_id";

    // Returns the persistent ID for this player. Generated on first call, then reused
    // across restarts (browser localStorage in WebGL builds, registry/file otherwise).
    public static string Get()
    {
        string id = PlayerPrefs.GetString(SESSION_ID_KEY, "");
        if (string.IsNullOrEmpty(id))
        {
            id = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(SESSION_ID_KEY, id);
            PlayerPrefs.Save();
        }
        return id;
    }
}
