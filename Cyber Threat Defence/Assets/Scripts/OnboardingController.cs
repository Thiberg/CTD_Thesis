using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OnboardingController : MonoBehaviour
{
    public static OnboardingController Instance { get; private set; }

    [Header("Panels")]
    public GameObject introPanel;
    public GameObject tooltipPanel;

    [Header("Text Refs")]
    public TextMeshProUGUI tooltipText;

    [Header("Buttons")]
    public Button introStartButton;
    public Button tooltipNextButton;

    [Header("Intro Copy")]
    [TextArea(3, 8)] public string introTitle = "Welcome to NovaCorp.";
    [TextArea(6, 16)] public string introBody =
        "You've just been hired as System Architect at NovaCorp, a growing tech company that depends on you to build and protect its digital infrastructure.\n\n" +
        "Your job is two-fold: keep the network operational and earning revenue, and defend it against the cyber threats targeting it daily — phishing emails, breach attempts, leaked credentials.\n\n" +
        "Every decision matters. The choices you make about which systems to deploy, how to connect them, how to secure them, and how to respond to suspicious messages will determine whether NovaCorp survives or falls.\n\n" +
        "Click START to receive your briefing.";
    public TextMeshProUGUI introTitleText;
    public TextMeshProUGUI introBodyText;

    [Header("Tooltip Copy")]
    [TextArea(3, 6)] public string tooltip1 =
        "Build your network by dragging nodes from the bar at the bottom onto the grid. There are four types: Services generate revenue, Servers route data, Databases store information, and Firewalls protect connected nodes. Each one shows its cost on the price tag.";
    [TextArea(3, 6)] public string tooltip2 =
        "Services don't earn money on their own. To generate revenue, a Service needs to be connected to a Server, which is connected to a Database. Break any link in this chain and revenue stops.";
    [TextArea(3, 6)] public string tooltip3 =
        "Connect two nodes by dragging from one of the small dots on a node's edge to a dot on another node. These connections route both your data AND any attackers who breach in — design them carefully.";
    [TextArea(3, 6)] public string tooltip4 =
        "Every node needs login credentials. When placing one, you'll choose to either generate a new unique password ($50) or reuse the last one (free). Sharing passwords is cheap, but if one shared node gets breached, the others start leaking too.";
    [TextArea(3, 6)] public string tooltip5 =
        "Click any placed node to open its inspector. From there you can see its current health and security level, sell it for a partial refund, upgrade it to boost its stats, or rotate its password ($50) to stop a credential leak.";
    [TextArea(3, 6)] public string tooltip6 =
        "Watch your inbox. Coworkers will email asking for help — Reply to legitimate requests. Attackers will send phishing emails disguised as real messages — Report those. Reply to phishing, and your reputation takes a hit. Ignoring mail entirely doesn't help either — misses count against you.";
    [TextArea(3, 6)] public string tooltip7 =
        "You are now all set up for the job. The clock starts when you press START GAME. You have 15 minutes to keep NovaCorp's infrastructure operational. The session ends in success if you survive — or in failure if reputation hits zero, or your revenue stream stays broken for over a minute. Good luck, Architect.";

    [Header("Button Labels")]
    public string nextLabel = "Got it";
    public string startGameLabel = "Start Game";

    public bool IsActive { get; private set; } = true;
    public static event Action OnOnboardingCompleted;

    private int currentTooltip = 0;
    private TextMeshProUGUI tooltipNextButtonLabel;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (introPanel != null) introPanel.SetActive(true);
        if (tooltipPanel != null) tooltipPanel.SetActive(false);

        if (introTitleText != null) introTitleText.text = introTitle;
        if (introBodyText != null)  introBodyText.text  = introBody;

        if (introStartButton != null)  introStartButton.onClick.AddListener(OnIntroStart);
        if (tooltipNextButton != null)
        {
            tooltipNextButton.onClick.AddListener(OnTooltipNext);
            tooltipNextButtonLabel = tooltipNextButton.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    private void OnIntroStart()
    {
        if (introPanel != null) introPanel.SetActive(false);
        currentTooltip = 1;
        ShowTooltip(tooltip1, false);
    }

    private void OnTooltipNext()
    {
        currentTooltip++;
        switch (currentTooltip)
        {
            case 2: ShowTooltip(tooltip2, false); break;
            case 3: ShowTooltip(tooltip3, false); break;
            case 4: ShowTooltip(tooltip4, false); break;
            case 5: ShowTooltip(tooltip5, false); break;
            case 6: ShowTooltip(tooltip6, false); break;
            case 7: ShowTooltip(tooltip7, true);  break;
            default: FinishOnboarding(); break;
        }
    }

    private void ShowTooltip(string text, bool isLast)
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(true);
        if (tooltipText != null)  tooltipText.text = text;
        if (tooltipNextButtonLabel != null)
            tooltipNextButtonLabel.text = isLast ? startGameLabel : nextLabel;
    }

    private void FinishOnboarding()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
        IsActive = false;
        OnOnboardingCompleted?.Invoke();
    }
}
