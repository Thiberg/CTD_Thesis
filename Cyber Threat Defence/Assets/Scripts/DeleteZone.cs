using UnityEngine;
using UnityEngine.EventSystems;

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