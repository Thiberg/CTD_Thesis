using UnityEngine;
using UnityEngine.EventSystems;

// Drag-to-delete tile. Refunds via DragableItem.GetSellRefund (60% of total invested
// for healthy nodes, 10% for compromised).
public class DeleteZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        if (eventData.pointerDrag.TryGetComponent(out DragableItem dragableItem))
        {
            if (dragableItem.hasBeenPlaced && dragableItem.nodeData != null)
            {
                GameManager.Instance?.AddBalance(dragableItem.GetSellRefund());
            }
            Destroy(dragableItem.gameObject);
        }
    }
}