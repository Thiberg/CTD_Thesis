using UnityEngine.EventSystems;
using UnityEngine;

// Attach this to every "Connection node" child of a Placement tile prefab.
public class ConnectionDot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    // The Placement tile hierarchy: Placement tile → [In-game container (PlacementSpot), Connection nodes…]
    private PlacementSpot GetOwnerSpot() =>
        transform.parent.GetComponentInChildren<PlacementSpot>();

    private DragableItem GetOwnerNode() =>
        GetOwnerSpot()?.GetComponentInChildren<DragableItem>();

    public void OnBeginDrag(PointerEventData eventData)
    {
        DragableItem owner = GetOwnerNode();
        if (owner == null || !owner.hasBeenPlaced) return;
        ConnectionManager.Instance?.BeginConnection(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ConnectionManager.Instance?.IsPending == true)
            ConnectionManager.Instance.UpdateGhostLine(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Only cancel if nothing completed the connection (OnDrop on target fires first)
        if (ConnectionManager.Instance?.IsPending == true)
            ConnectionManager.Instance.CancelConnection();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (ConnectionManager.Instance == null || !ConnectionManager.Instance.IsPending) return;

        DragableItem owner = GetOwnerNode();
        if (owner == null || !owner.hasBeenPlaced) return;

        ConnectionManager.Instance.CompleteConnection(this);
    }
}
