using UnityEngine.EventSystems;
using UnityEngine;

// Drag handler on each gateway dot. Attach to every "Connection node" child of a
// Placement tile prefab. Coordinates with ConnectionManager to draw a ghost line during
// drag and finalise the connection on drop.
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
