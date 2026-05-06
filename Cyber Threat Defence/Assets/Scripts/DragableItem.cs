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

    public int CurrentHealth { get; private set; }
    public bool IsCompromised { get; private set; }
    public bool IsUnderAttack { get; private set; }

    public GameObject prefab;
    public bool isSource = true;

    [Header("Node Info")]
    public NodeData nodeData;

    private Color originalColor = Color.white;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (image != null) originalColor = image.color;
    }

    private void Update()
    {
        if (image == null || !hasBeenPlaced) return;

        if (IsCompromised)
        {
            image.color = Color.red;
            return;
        }

        if (IsUnderAttack)
        {
            float t = (Mathf.Sin(Time.time * 6f) + 1f) * 0.5f;
            image.color = Color.Lerp(originalColor, Color.red, t);
            return;
        }

        image.color = originalColor;
    }

    public void InitialiseHealth()
    {
        if (nodeData != null)
            CurrentHealth = nodeData.maxHealth;
    }

    public void ApplyDamage(int amount)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        if (CurrentHealth == 0) IsCompromised = true;
    }

    public void SetCompromised(bool value)
    {
        IsCompromised = value;
    }

    public void SetUnderAttack(bool value)
    {
        IsUnderAttack = value;
    }

    private void OnDestroy()
    {
        GameManager.Instance?.UnregisterNode(this);
        CredentialManager.Instance?.UnregisterNode(this);
        ConnectionManager.Instance?.RemoveConnectionsFor(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!hasBeenPlaced) return;

        NodeInspectorPanel inspector = FindFirstObjectByType<NodeInspectorPanel>(FindObjectsInactive.Include);
        if (inspector != null)
        {
            PlacementSpot spot = GetComponentInParent<PlacementSpot>();
            inspector.Show(this, spot);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        NodeInspectorPanel inspector = FindFirstObjectByType<NodeInspectorPanel>(FindObjectsInactive.Include);
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