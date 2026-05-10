using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EndScreenController : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panelRoot;

    [Header("Header")]
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI subHeaderText;

    [Header("Metric Rows (one TMP per metric)")]
    public TextMeshProUGUI credentialUniquenessText;
    public TextMeshProUGUI criticalProtectionText;
    public TextMeshProUGUI firewallLoadText;
    public TextMeshProUGUI revenueRateText;
    public TextMeshProUGUI reputationText;
    public TextMeshProUGUI phishingAccuracyText;
    public TextMeshProUGUI companyValueText;

    [Header("Total")]
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI totalScoreSummaryText;

    [Header("Actions")]
    public Button restartButton;
    public Button questionnaireButton;

    [Header("Views (toggled by Start Questionnaire button)")]
    [Tooltip("Container holding the metrics breakdown — shown by default after game ends.")]
    public GameObject metricsView;
    [Tooltip("Container holding the session IDs view — shown when Start Questionnaire is clicked.")]
    public GameObject sessionIdsView;
    [Tooltip("TMP that lists all stored session IDs.")]
    public TextMeshProUGUI sessionIdsListText;

    [Header("Copy")]
    [TextArea(2, 4)] public string winHeader = "Network Secured";
    [TextArea(2, 4)] public string winSubHeader = "You held NovaCorp's infrastructure together for the full session.";
    [TextArea(2, 4)] public string loseHeader = "Network Compromised";

    private void Start()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
        if (questionnaireButton != null) questionnaireButton.onClick.AddListener(OnQuestionnaireClicked);
    }

    private void OnEnable()
    {
        GameManager.OnGameWon  += HandleWin;
        GameManager.OnGameOver += HandleLose;
    }

    private void OnDisable()
    {
        GameManager.OnGameWon  -= HandleWin;
        GameManager.OnGameOver -= HandleLose;
    }

    private void HandleWin()
    {
        if (headerText != null)    headerText.text    = winHeader;
        if (subHeaderText != null) subHeaderText.text = winSubHeader;
        PopulateMetrics();
        ShowMetricsView();
        if (panelRoot != null) panelRoot.SetActive(true);
    }

    private void HandleLose(string reason)
    {
        if (headerText != null)    headerText.text    = loseHeader;
        if (subHeaderText != null) subHeaderText.text = reason;
        PopulateMetrics();
        ShowMetricsView();
        if (panelRoot != null) panelRoot.SetActive(true);
    }

    private void ShowMetricsView()
    {
        if (metricsView != null)    metricsView.SetActive(true);
        if (sessionIdsView != null) sessionIdsView.SetActive(false);
    }

    private void OnQuestionnaireClicked()
    {
        if (metricsView != null)    metricsView.SetActive(false);
        if (sessionIdsView != null) sessionIdsView.SetActive(true);

        if (sessionIdsListText != null)
        {
            var ids = SessionIdentity.GetAllSessionIds();
            if (ids.Count == 0)
            {
                sessionIdsListText.text = "(no sessions recorded)";
            }
            else
            {
                System.Text.StringBuilder sb = new();
                foreach (string id in ids) sb.AppendLine(id);
                sessionIdsListText.text = sb.ToString();
            }
        }
    }

    private void PopulateMetrics()
    {
        if (MetricsTracker.Instance == null) return;
        MetricsTracker.FinalScore s = MetricsTracker.Instance.Compute();
        Set(credentialUniquenessText, s.credentialUniqueness);
        Set(criticalProtectionText,   s.criticalProtection);
        Set(firewallLoadText,         s.firewallLoad);
        Set(revenueRateText,          s.revenueRate);
        Set(reputationText,           s.reputation);
        Set(phishingAccuracyText,     s.phishingAccuracy);
        Set(companyValueText,         s.companyValue);
        if (totalScoreText != null)        totalScoreText.text        = $"{s.totalScore:P0}";
        if (totalScoreSummaryText != null) totalScoreSummaryText.text = SummariseTotal(s.totalScore);
    }

    private static void Set(TextMeshProUGUI tmp, float value)
    {
        if (tmp != null) tmp.text = $"{value:P0}";
    }

    private static string SummariseTotal(float total)
    {
        if (total >= 0.85f) return "Exemplary architect — the network would inspire textbooks.";
        if (total >= 0.65f) return "Strong work — you held the line through escalating threats.";
        if (total >= 0.45f) return "Mixed performance — clear strengths, clear room to grow.";
        if (total >= 0.25f) return "Architecture under strain — the systems learned a hard lesson.";
        return "Crisis mode — most decisions tilted reactive over structural.";
    }

    private void OnRestart()
    {
        Scene s = SceneManager.GetActiveScene();
        SceneManager.LoadScene(s.buildIndex);
    }
}
