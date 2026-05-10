// Plain data record for a connection between two nodes.
// Owned and tracked by ConnectionManager.
public class NodeConnection
{
    public DragableItem NodeA { get; }
    public DragableItem NodeB { get; }
    public UnityEngine.GameObject LineObject { get; set; }

    public NodeConnection(DragableItem a, DragableItem b)
    {
        NodeA = a;
        NodeB = b;
    }

    public bool Involves(DragableItem node) => NodeA == node || NodeB == node;
    public DragableItem OtherNode(DragableItem node) => NodeA == node ? NodeB : NodeA;
}
