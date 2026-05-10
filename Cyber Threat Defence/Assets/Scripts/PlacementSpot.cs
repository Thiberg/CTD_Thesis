using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Drop target on the grid. On first placement: deducts cost, registers with GameManager,
// shows the credential popup. Polls its child node every Update to swap container sprite
// based on healthy / under-attack / compromised state.
public class PlacementSpot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Container Visuals")]
    public Image containerImage;
    public Sprite emptySprite;
    public Sprite occupiedSprite;
    public Sprite breachedSprite;

    private void Update()
    {
        if (containerImage == null) return;

        DragableItem child = GetComponentInChildren<DragableItem>();
        Sprite target = emptySprite;
        if (child != null && child.hasBeenPlaced)
        {
            bool showBreached = child.IsCompromised || child.IsUnderAttack;
            target = showBreached ? breachedSprite : occupiedSprite;
        }

        if (target != null && containerImage.sprite != target)
            containerImage.sprite = target;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        if (transform.childCount > 0) return;
        if (!eventData.pointerDrag.TryGetComponent(out DragableItem dragableItem)) return;

        bool isFreshPlacement = !dragableItem.hasBeenPlaced;

        if (isFreshPlacement && dragableItem.nodeData != null)
        {
            if (GameManager.Instance != null && !GameManager.Instance.CanAfford(dragableItem.nodeData.cost))
                return;
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