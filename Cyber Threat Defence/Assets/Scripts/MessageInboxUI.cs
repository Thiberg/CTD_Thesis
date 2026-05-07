using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageInboxUI : MonoBehaviour
{
    [Header("Inbox List")]
    [Tooltip("Scroll View Content with a layout group. Existing children are wiped and replaced on refresh.")]
    public RectTransform listContainer;
    [Tooltip("Prefab spawned per inbox row. Expects a Button at root and a TextMeshProUGUI in children.")]
    public GameObject messageRowPrefab;

    [Header("Body View")]
    public TextMeshProUGUI senderText;
    public TextMeshProUGUI subjectText;
    public TextMeshProUGUI bodyText;

    [Header("Actions")]
    public Button replyButton;
    public Button reportButton;

    [Header("Empty State (optional)")]
    public GameObject emptyStateObject;

    private MessageData selected;
    private readonly List<GameObject> rowInstances = new();

    private void Start()
    {
        if (replyButton != null) replyButton.onClick.AddListener(() => Respond(MessageAction.Reply));
        if (reportButton != null) reportButton.onClick.AddListener(() => Respond(MessageAction.Report));
    }

    private void OnEnable()
    {
        MessageManager.OnInboxChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        MessageManager.OnInboxChanged -= Refresh;
    }

    private void Refresh()
    {
        foreach (GameObject go in rowInstances) if (go != null) Destroy(go);
        rowInstances.Clear();

        if (MessageManager.Instance == null) { UpdateBodyView(); return; }

        IReadOnlyList<MessageData> inbox = MessageManager.Instance.Inbox;
        bool empty = inbox.Count == 0;

        if (emptyStateObject != null) emptyStateObject.SetActive(empty);

        if (selected != null)
        {
            bool stillPresent = false;
            for (int i = 0; i < inbox.Count; i++)
                if (inbox[i] == selected) { stillPresent = true; break; }
            if (!stillPresent) selected = null;
        }

        if (listContainer != null)
        {
            for (int i = 0; i < inbox.Count; i++)
                rowInstances.Add(CreateRow(inbox[i], i));
        }

        UpdateBodyView();
    }

    private GameObject CreateRow(MessageData msg, int index)
    {
        if (messageRowPrefab == null) return null;

        GameObject row = Instantiate(messageRowPrefab, listContainer);
        row.name = $"Msg_{index}_{msg.sender}";

        TextMeshProUGUI label = row.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = msg.subject;

        Button btn = row.GetComponent<Button>();
        if (btn == null) btn = row.GetComponentInChildren<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            MessageData captured = msg;
            btn.onClick.AddListener(() => Select(captured));
        }

        return row;
    }

    private void Select(MessageData msg)
    {
        selected = msg;
        Refresh();
    }

    private void UpdateBodyView()
    {
        bool has = selected != null;

        if (senderText  != null) senderText.text  = has ? "From: " + selected.sender : "";
        if (subjectText != null) subjectText.text = has ? selected.subject           : "";
        if (bodyText    != null) bodyText.text    = has ? selected.body              : "";

        if (replyButton  != null) replyButton.interactable  = has;
        if (reportButton != null) reportButton.interactable = has;
    }

    private void Respond(MessageAction action)
    {
        if (selected == null) return;
        MessageManager.Instance?.RespondTo(selected, action);
    }
}
