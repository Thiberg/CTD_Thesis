using System.Collections.Generic;
using UnityEngine;

public enum NodeType { Service, Server, Database, Firewall }

[System.Serializable]
public class UpgradeStep
{
    public int costToUpgrade;
    public int newMaxHealth;
    public int securityBoost = 1;
    public float newRevenuePerSecond;  // Service: absolute new rate. 0 = no change.
    public float newUpkeepPerSecond;   // Server/Database (flat) or Firewall (per-connection). 0 = no change.
    [Range(0f, 1f)] public float firewallReductionBonus; // Firewall only — adds to flat reduction.
}

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
    public float upkeepPerSecond; // Firewall: cost per connected node per second

    [Header("Combat")]
    public int maxHealth;
    public int securityLevel; // 1–5: higher = harder to breach

    [Header("Upgrades")]
    public List<UpgradeStep> upgrades = new();
}
