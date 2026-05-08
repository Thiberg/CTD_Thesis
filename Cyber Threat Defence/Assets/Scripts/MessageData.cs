using UnityEngine;

public enum MessageType { Normal, Phishing }
public enum MessageAction { Reply, Report }

[CreateAssetMenu(fileName = "MessageData", menuName = "Architect/Message Data")]
public class MessageData : ScriptableObject
{
    public string sender;
    public string subject;
    [TextArea(3, 8)] public string body;
    public MessageType type;

    [Tooltip("0 = eligible in any stage. 1/2/3 = locked to that stage only.")]
    public int stage = 0;
}
