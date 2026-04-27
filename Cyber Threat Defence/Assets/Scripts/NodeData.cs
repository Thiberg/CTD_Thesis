using UnityEngine;

[CreateAssetMenu(fileName = "NodeData", menuName = "Architect/Node Data")]
public class NodeData : ScriptableObject
{
    public string nodeName;
    public string tier;
    public string description;
    public Sprite icon;
}
