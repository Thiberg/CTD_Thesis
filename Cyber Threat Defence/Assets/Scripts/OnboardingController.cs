using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Walks the player through the introductory screens. Each screen shows text plus optional
// arrow GameObjects pointing at relevant UI. While IsActive is true, ThreatManager and
// MessageManager don't spawn anything and the GameManager session timer is paused.
public class OnboardingController : MonoBehaviour
{
    public static OnboardingController Instance { get; private set; }

    [Serializable]
    public class OnboardingScreen
    {
        [TextArea(3, 6)] public string text;
        public GameObject[] arrows;
    }

    [Header("Panels")]
    [Tooltip("Optional separate intro panel shown before the first screen. Leave empty to start directly at screen 1.")]
    public GameObject introPanel;
    public GameObject tooltipPanel;

    [Header("Text Refs")]
    public TextMeshProUGUI tooltipText;

    [Header("Buttons")]
    public Button introStartButton;
    public Button tooltipNextButton;

    [Header("Intro Copy (only used if introPanel is wired)")]
    [TextArea(3, 8)]  public string introTitle = "Welcome to NovaCorp.";
    [TextArea(6, 16)] public string introBody  = "Briefing follows.";
    public TextMeshProUGUI introTitleText;
    public TextMeshProUGUI introBodyText;

    [Header("Screens (text + arrows per step)")]
    public OnboardingScreen[] screens;

    [Header("Button Labels")]
    public string nextLabel      = "Got it";
    public string startGameLabel = "Start Game";

    public bool IsActive { get; private set; } = true;
    public static event Action OnOnboardingCompleted;

    private int currentScreenIndex = -1;
    private TextMeshProUGUI tooltipNextButtonLabel;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
        HideAllArrows();

        if (introTitleText != null) introTitleText.text = introTitle;
        if (introBodyText != null)  introBodyText.text  = introBody;

        if (introStartButton != null) introStartButton.onClick.AddListener(OnIntroStart);
        if (tooltipNextButton != null)
        {
            tooltipNextButton.onClick.AddListener(OnTooltipNext);
            tooltipNextButtonLabel = tooltipNextButton.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (introPanel != null)
            introPanel.SetActive(true);
        else
            ShowScreen(0);
    }

    private void OnIntroStart()
    {
        if (introPanel != null) introPanel.SetActive(false);
        ShowScreen(0);
    }

    private void OnTooltipNext()
    {
        int next = currentScreenIndex + 1;
        if (screens == null || next >= screens.Length)
        {
            FinishOnboarding();
            return;
        }
        ShowScreen(next);
    }

    private void ShowScreen(int index)
    {
        if (screens == null || index < 0 || index >= screens.Length) return;
        currentScreenIndex = index;

        OnboardingScreen screen = screens[index];
        bool isLast = index == screens.Length - 1;

        if (tooltipPanel != null) tooltipPanel.SetActive(true);
        if (tooltipText != null && screen != null) tooltipText.text = screen.text;
        if (tooltipNextButtonLabel != null)
            tooltipNextButtonLabel.text = isLast ? startGameLabel : nextLabel;

        HideAllArrows();
        if (screen != null && screen.arrows != null)
        {
            foreach (GameObject arrow in screen.arrows)
                if (arrow != null) arrow.SetActive(true);
        }
    }

    private void HideAllArrows()
    {
        if (screens == null) return;
        foreach (OnboardingScreen screen in screens)
        {
            if (screen == null || screen.arrows == null) continue;
            foreach (GameObject arrow in screen.arrows)
                if (arrow != null) arrow.SetActive(false);
        }
    }

    private void FinishOnboarding()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
        HideAllArrows();
        IsActive = false;
        OnOnboardingCompleted?.Invoke();
    }

    [ContextMenu("Populate Default Screens (10)")]
    private void PopulateDefaultScreens()
    {
        screens = new OnboardingScreen[]
        {
            new OnboardingScreen { text = "Welcome to NovaCorp. You are the new system architect. Everything you need to build the company's network is right here in the toolbar. Once the introduction is over, you can drag a node onto the grid to get started." },
            new OnboardingScreen { text = "This is a Service node. It is the customer-facing part of the network, the thing that actually generates revenue for NovaCorp. On its own it does nothing. It needs infrastructure behind it to function." },
            new OnboardingScreen { text = "A Service needs a Server to process its requests, and that Server needs a Database to store its data. Connect the three and you have a working revenue chain. Leave any link missing and the money stops flowing." },
            new OnboardingScreen { text = "This is a Firewall. It does not generate revenue, but it protects the nodes connected to it from attacks. The more nodes it covers, the harder it works. Spread your firewalls out rather than relying on just one." },
            new OnboardingScreen { text = "This is your workspace. Build your network here, keep the chains connected, and protect what matters. It will get busier." },
            new OnboardingScreen { text = "Nodes connect through these dots on their edges. Connect a Service to a Server, and that Server to a Database. That chain is what keeps NovaCorp running." },
            new OnboardingScreen { text = "You will also be getting emails throughout the day. Coworkers asking for help, access requests, that kind of thing. Reply to the ones that are legitimate. Some of them will not be — flag those as phishing. Your reputation depends on getting this right." },
            new OnboardingScreen { text = "Click any node you have placed to open its inspector. From here you can check its health, sell it for a partial refund, upgrade it, or rotate its password for $50 if you think it has been compromised." },
            new OnboardingScreen { text = "Keep an eye on these. If your reputation hits zero, it is over. If your revenue stream stays broken for more than a minute, it is also over. Stay on top of both." },
            new OnboardingScreen { text = "That is everything. Fifteen minutes, one network, and a company counting on you. Good luck, Architect." },
        };
    }
}
