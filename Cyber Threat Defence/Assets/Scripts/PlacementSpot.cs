using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementSpot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        if (transform.childCount > 0) return;

        if (eventData.pointerDrag.TryGetComponent(out DragableItem dragableItem))
        {
            dragableItem.parentAfterDrag = transform;
            dragableItem.wasDroppedOnValidSpot = true;
            dragableItem.hasBeenPlaced = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
{
    Debug.Log("Spot clicked — children: " + transform.childCount);
    
    DragableItem node = GetComponentInChildren<DragableItem>();
    if (node == null) return;
    if (!node.hasBeenPlaced) return;

    NodeInspectorPanel inspector = FindObjectOfType<NodeInspectorPanel>();
    if (inspector != null)
    {
        inspector.Show(node, this);
    }
}
}