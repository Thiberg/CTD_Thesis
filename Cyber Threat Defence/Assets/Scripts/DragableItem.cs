using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    private RectTransform rectTransform;

    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public bool wasDroppedOnValidSpot = false;
    [HideInInspector] public bool hasBeenPlaced = false;

    public GameObject prefab;
    public bool isSource = true;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Spawn replacement if this is a source item
        if (isSource)
        {
            GameObject replacement = Instantiate(prefab, transform.parent);
            replacement.transform.SetSiblingIndex(transform.GetSiblingIndex());
            replacement.GetComponent<DragableItem>().isSource = true;

            isSource = false;
        }

        wasDroppedOnValidSpot = false;

        // Store current parent as fallback location
        parentAfterDrag = transform.parent;

        // Move to top canvas layer for proper dragging
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        rectTransform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;

        // Invalid drop handling
        if (!wasDroppedOnValidSpot)
        {
            if (hasBeenPlaced)
            {
                // Return to previous valid position
                transform.SetParent(parentAfterDrag);
            }
            else
            {
                // Destroy if never placed before
                Destroy(gameObject);
            }

            return;
        }

        // Valid placement
        transform.SetParent(parentAfterDrag);
    }
}