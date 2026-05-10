using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Owns all node-to-node connection lines. Handles drag-to-connect via ConnectionDot,
// renders lines as UI Images, and exposes graph queries used by ThreatManager and RevenueManager
// (IsChainIntact, GetConnectedNodes, GetConnectionCount).
public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance { get; private set; }

    [Header("UI")]
    [Tooltip("Full-screen transparent panel for connection lines. Auto-created if left empty.")]
    [SerializeField] private RectTransform connectionsParent;

    [Header("Appearance")]
    [SerializeField] private Color lineColor = new Color(0.4f, 0.85f, 1f, 1f);
    [SerializeField] private float lineThickness = 22f;

    public RectTransform ConnectionsParent => connectionsParent;

    private ConnectionDot  pendingFrom;
    public  bool           IsPending => pendingFrom != null;

    private Image         ghostLine;
    private RectTransform ghostLineRect;

    private readonly List<NodeConnection> connections = new();

    // ── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (connectionsParent == null)
            connectionsParent = CreateConnectionsLayer();

        if (connectionsParent == null)
        {
            Debug.LogError("[ConnectionManager] Could not find or create a connections parent panel.");
            return;
        }

        // Force to top of Canvas so lines always render above the background.
        connectionsParent.transform.SetAsLastSibling();

        CreateGhostLine();
    }

    private void LateUpdate()
    {
        for (int i = connections.Count - 1; i >= 0; i--)
        {
            var conn = connections[i];
            if (conn.NodeA == null || conn.NodeB == null) { RemoveAt(i); continue; }
            if (conn.LineObject == null) continue;

            SetLineBetween(
                conn.LineObject.GetComponent<RectTransform>(),
                ScreenPos(conn.NodeA.GetComponent<RectTransform>()),
                ScreenPos(conn.NodeB.GetComponent<RectTransform>()));

            UpdateLineColor(conn);
        }
    }

    // Reactive line tinting: red when breached, pulsing red while under attack, base colour otherwise.
    private void UpdateLineColor(NodeConnection conn)
    {
        Image img = conn.LineObject.GetComponent<Image>();
        if (img == null) return;

        bool breached = conn.NodeA.IsCompromised || conn.NodeB.IsCompromised;
        if (breached) { img.color = Color.red; return; }

        bool attacked = conn.NodeA.IsUnderAttack || conn.NodeB.IsUnderAttack;
        if (attacked)
        {
            float t = (Mathf.Sin(Time.time * 6f) + 1f) * 0.5f;
            img.color = Color.Lerp(lineColor, Color.red, t);
            return;
        }

        img.color = lineColor;
    }

    // ── Connection flow ──────────────────────────────────────────────────────

    public void BeginConnection(ConnectionDot from)
    {
        pendingFrom = from;
        ghostLine.gameObject.SetActive(true);
    }

    public void UpdateGhostLine(Vector2 screenPos)
    {
        if (!IsPending) return;
        SetLineBetween(ghostLineRect,
            ScreenPos(pendingFrom.GetComponent<RectTransform>()),
            screenPos);
    }

    public void CancelConnection()
    {
        pendingFrom = null;
        ghostLine.gameObject.SetActive(false);
    }

    public void CompleteConnection(ConnectionDot toDot)
    {
        if (!IsPending) return;

        DragableItem nodeA = GetNode(pendingFrom);
        DragableItem nodeB = GetNode(toDot);

        pendingFrom = null;
        ghostLine.gameObject.SetActive(false);

        if (nodeA == null || nodeB == null)
        {
            Debug.LogWarning("[ConnectionManager] CompleteConnection: one or both nodes are null — no line drawn.");
            return;
        }
        if (nodeA == nodeB) return;
        if (AlreadyConnected(nodeA, nodeB)) return;

        NodeConnection conn = new(nodeA, nodeB);
        connections.Add(conn);

        // ── Spawn permanent line ──
        GameObject lineGO = new("Connection_Line");
        lineGO.transform.SetParent(connectionsParent, false);

        Image img = lineGO.AddComponent<Image>();
        img.color        = lineColor;
        img.raycastTarget = false;

        RectTransform rt = lineGO.GetComponent<RectTransform>();
        conn.LineObject  = lineGO;

        Vector2 posA = ScreenPos(nodeA.GetComponent<RectTransform>());
        Vector2 posB = ScreenPos(nodeB.GetComponent<RectTransform>());
        SetLineBetween(rt, posA, posB);
    }

    public void RemoveConnectionsFor(DragableItem node)
    {
        for (int i = connections.Count - 1; i >= 0; i--)
            if (connections[i].Involves(node)) RemoveAt(i);
    }

    // ── Revenue query ────────────────────────────────────────────────────────

    public bool IsChainIntact(DragableItem service)
    {
        foreach (var c1 in connections)
        {
            if (!c1.Involves(service)) continue;
            DragableItem server = c1.OtherNode(service);
            if (server == null || server.IsCompromised) continue;
            if (server.nodeData?.nodeType != NodeType.Server) continue;

            foreach (var c2 in connections)
            {
                if (!c2.Involves(server)) continue;
                DragableItem db = c2.OtherNode(server);
                if (db == null || db == service || db.IsCompromised) continue;
                if (db.nodeData?.nodeType == NodeType.Database) return true;
            }
        }
        return false;
    }

    public List<DragableItem> GetConnectedNodes(DragableItem node, NodeType type)
    {
        var result = new List<DragableItem>();
        foreach (var conn in connections)
        {
            if (!conn.Involves(node)) continue;
            DragableItem other = conn.OtherNode(node);
            if (other != null && other.nodeData?.nodeType == type) result.Add(other);
        }
        return result;
    }

    public int GetConnectionCount(DragableItem node)
    {
        int count = 0;
        foreach (var conn in connections)
            if (conn.Involves(node)) count++;
        return count;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static DragableItem GetNode(ConnectionDot dot) =>
        dot.transform.parent.GetComponentInChildren<PlacementSpot>()
            ?.GetComponentInChildren<DragableItem>();

    private bool AlreadyConnected(DragableItem a, DragableItem b)
    {
        foreach (var conn in connections)
            if ((conn.NodeA == a && conn.NodeB == b) ||
                (conn.NodeA == b && conn.NodeB == a))
                return true;
        return false;
    }

    private void RemoveAt(int i)
    {
        if (connections[i].LineObject != null) Destroy(connections[i].LineObject);
        connections.RemoveAt(i);
    }

    // Converts a RectTransform's world position to screen pixels (correct for Screen Space Overlay).
    private static Vector2 ScreenPos(RectTransform rt) =>
        RectTransformUtility.WorldToScreenPoint(null, rt.position);

    // All inputs and outputs are screen-space pixels (origin = bottom-left of screen).
    private void SetLineBetween(RectTransform rt, Vector2 screenA, Vector2 screenB)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            connectionsParent, screenA, null, out Vector2 localA);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            connectionsParent, screenB, null, out Vector2 localB);

        Vector2 dir   = localB - localA;
        float   dist  = dir.magnitude;
        float   angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        rt.pivot            = new Vector2(0f, 0.5f);
        rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = localA;
        rt.sizeDelta        = new Vector2(dist, lineThickness);
        rt.localRotation    = Quaternion.Euler(0f, 0f, angle);
    }

    private void CreateGhostLine()
    {
        GameObject go   = new("_GhostLine");
        go.transform.SetParent(connectionsParent, false);
        ghostLine            = go.AddComponent<Image>();
        ghostLine.color      = new Color(0.5f, 0.9f, 1f, 0.8f);
        ghostLine.raycastTarget = false;
        ghostLineRect        = go.GetComponent<RectTransform>();
        go.SetActive(false);
    }

    // Auto-creates a full-screen transparent panel as the topmost child of the Screen Space Overlay canvas.
    private static RectTransform CreateConnectionsLayer()
    {
        Canvas target = null;
        foreach (Canvas c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay && c.isRootCanvas)
            { target = c; break; }
        }
        if (target == null) target = FindAnyObjectByType<Canvas>();
        if (target == null) { Debug.LogError("[ConnectionManager] No Canvas found in scene!"); return null; }

        GameObject panel  = new("_ConnectionsLayer");
        panel.transform.SetParent(target.transform, false);

        RectTransform rt  = panel.AddComponent<RectTransform>();
        rt.pivot          = new Vector2(0.5f, 0.5f);
        rt.anchorMin      = Vector2.zero;
        rt.anchorMax      = Vector2.one;
        rt.offsetMin      = Vector2.zero;
        rt.offsetMax      = Vector2.zero;

        // Render on top of everything so we can confirm lines exist visually.
        // Move behind nodes later by assigning connectionsParent in the Inspector instead.
        panel.transform.SetAsLastSibling();

        return rt;
    }
}
