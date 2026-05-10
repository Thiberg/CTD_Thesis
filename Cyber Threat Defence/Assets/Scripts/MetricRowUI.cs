using UnityEngine;
using TMPro;

public class MetricRowUI : MonoBehaviour
{
    [Header("Refs")]
    public TextMeshProUGUI labelText;
    public TextMeshProUGUI percentText;
    public TextMeshProUGUI explanationText;
    [Tooltip("Child of the bar background. Should be anchored stretch-to-parent (anchorMin 0,0; anchorMax 1,1; offsets 0). Its anchorMax.x is animated to match the score.")]
    public RectTransform barFill;

    public void SetData(string label, float value, string explanation)
    {
        if (labelText != null)       labelText.text       = label;
        if (percentText != null)     percentText.text     = $"{value:P0}";
        if (explanationText != null) explanationText.text = explanation;

        if (barFill != null)
        {
            Vector2 max = barFill.anchorMax;
            max.x = Mathf.Clamp01(value);
            barFill.anchorMax = max;
        }
    }
}
