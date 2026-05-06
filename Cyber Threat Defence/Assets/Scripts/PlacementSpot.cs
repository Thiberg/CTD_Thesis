using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementSpot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        if (transform.childCount > 0) return;
        if (!eventData.pointerDrag.TryGetComponent(out DragableItem dragableItem)) return;

        bool isFreshPlacement = !dragableItem.hasBeenPlaced;

        if (isFreshPlacement && dragableItem.nodeData != null)
        {
            if (GameManager.Instance != null && !GameManager.Instance.CanAfford(dragableItem.nodeData.cost))
            {
                Debug.Log("Cannot afford " + dragableItem.nodeData.nodeName);
                return;
            }
            GameManager.Instance?.DeductBalance(dragableItem.nodeData.cost);
        }

        dragableItem.parentAfterDrag = transform;
        dragableItem.wasDroppedOnValidSpot = true;

        if (isFreshPlacement)
        {
            dragableItem.hasBeenPlaced = true;
            dragableItem.InitialiseHealth();
            GameManager.Instance?.RegisterNode(dragableItem);
            CredentialPopup.Instance?.Show(dragableItem);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
{
    Debug.Log("Spot clicked — children: " + transform.childCount);
    
    DragableItem node = GetComponentInChildren<DragableItem>();
    if (node == null) return;
    if (!node.hasBeenPlaced) return;

    NodeInspectorPanel inspector = FindFirstObjectByType<NodeInspectorPanel>(FindObjectsInactive.Include);
    if (inspector != null)
    {
        inspector.Show(node, this);
    }
}
}