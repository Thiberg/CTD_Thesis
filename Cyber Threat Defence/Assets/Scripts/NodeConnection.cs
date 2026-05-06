public enum ConnectionDirection { TwoWay, AtoB, BtoA }

public class NodeConnection
{
    public DragableItem NodeA { get; }
    public DragableItem NodeB { get; }
    public ConnectionDirection Direction { get; set; } = ConnectionDirection.TwoWay;
    public UnityEngine.GameObject LineObject { get; set; }

    public NodeConnection(DragableItem a, DragableItem b)
    {
        NodeA = a;
        NodeB = b;
    }

    public bool Involves(DragableItem node) => NodeA == node || NodeB == node;

    public DragableItem OtherNode(DragableItem node) => NodeA == node ? NodeB : NodeA;

    // Can data flow from → to through this connection?
    public bool CanFlowFrom(DragableItem from, DragableItem to)
    {
        if (!Involves(from) || !Involves(to)) return false;
        return Direction switch
        {
            ConnectionDirection.TwoWay => true,
            ConnectionDirection.AtoB   => from == NodeA && to == NodeB,
            ConnectionDirection.BtoA   => from == NodeB && to == NodeA,
            _                          => false,
        };
    }
}
