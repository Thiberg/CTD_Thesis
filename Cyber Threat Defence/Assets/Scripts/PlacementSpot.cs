using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementSpot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        // Reject if already occupied
        if (transform.childCount > 0)
        {
            return;
        }

        if (eventData.pointerDrag.TryGetComponent(out DragableItem dragableItem))
        {
            dragableItem.parentAfterDrag = transform;
            dragableItem.wasDroppedOnValidSpot = true;
            dragableItem.hasBeenPlaced = true;
        }
    }
}