using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Image image;
    private RectTransform rectTransform;

    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public bool wasDroppedOnValidSpot = false;
    [HideInInspector] public bool hasBeenPlaced = false;

    public GameObject prefab;
    public bool isSource = true;

    [Header("Node Info")]
    public NodeData nodeData;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!hasBeenPlaced) return;

        NodeInspectorPanel inspector = FindObjectOfType<NodeInspectorPanel>(true);
        if (inspector != null)
        {
            PlacementSpot spot = GetComponentInParent<PlacementSpot>();
            inspector.Show(this, spot);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        NodeInspectorPanel inspector = FindObjectOfType<NodeInspectorPanel>(true);
        if (inspector != null) inspector.Hide();

        // Spawn a fresh replacement in the taskbar slot
        if (isSource)
        {
            GameObject replacement = Instantiate(prefab, transform.parent);
            replacement.transform.SetSiblingIndex(transform.GetSiblingIndex());

            DragableItem rep = replacement.GetComponent<DragableItem>();
            rep.isSource = true;
            rep.hasBeenPlaced = false; // Ensure replacement never thinks it's placed

            isSource = false;
        }

        wasDroppedOnValidSpot = false;
        parentAfterDrag = transform.parent;

        // Bring to front for dragging
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

        if (!wasDroppedOnValidSpot)
        {
            if (hasBeenPlaced)
            {
                // Return to its grid spot
                transform.SetParent(parentAfterDrag);
            }
            else
            {
                // Destroy if it never made it to the grid
                Destroy(gameObject);
            }
            return;
        }

        // Snap into the valid placement spot
        transform.SetParent(parentAfterDrag);
    }
}