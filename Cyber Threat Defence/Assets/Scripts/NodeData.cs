using UnityEngine;

public enum NodeType { Service, Server, Database, Firewall }

[CreateAssetMenu(fileName = "NodeData", menuName = "Architect/Node Data")]
public class NodeData : ScriptableObject
{
    public string nodeName;
    public NodeType nodeType;
    public int tier;
    public string description;
    public Sprite icon;

    [Header("Economy")]
    public int cost;
    public float revenuePerSecond;

    [Header("Combat")]
    public int maxHealth;
    public int securityLevel; // 1–5: higher = harder to breach
}
