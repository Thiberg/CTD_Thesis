using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("HUD Text Fields")]
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI reputationText;
    public TextMeshProUGUI timerText;

    private void OnEnable()
    {
        GameManager.OnBalanceChanged += UpdateBalance;
        GameManager.OnReputationChanged += UpdateReputation;
        GameManager.OnTimerChanged += UpdateTimer;
    }

    private void OnDisable()
    {
        GameManager.OnBalanceChanged -= UpdateBalance;
        GameManager.OnReputationChanged -= UpdateReputation;
        GameManager.OnTimerChanged -= UpdateTimer;
    }

    private void UpdateBalance(float value) =>
        balanceText.text = "$" + Mathf.FloorToInt(value).ToString();

    private void UpdateReputation(float value) =>
        reputationText.text = "Rep: " + Mathf.FloorToInt(value).ToString();

    private void UpdateTimer(float value)
    {
        int minutes = Mathf.FloorToInt(value / 60f);
        int seconds = Mathf.FloorToInt(value % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
