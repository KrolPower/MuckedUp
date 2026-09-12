using TMPro;
using UnityEngine;

public class MarketSquareUI : MonoBehaviour
{
    public static MarketSquareUI Instance;

    public GameObject countdownContainer;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI readyStatusText;

    private void Awake()
    {
        Instance = this;
        if (countdownContainer != null)
        {
            countdownContainer.SetActive(false);
        }
    }

    public void UpdateCountdown(float secondsLeft, int readyCount, int totalCount)
    {
        if (countdownContainer != null)
        {
            countdownContainer.SetActive(secondsLeft > 0f);
        }

        if (countdownText != null)
        {
            int ceilSeconds = Mathf.CeilToInt(secondsLeft);
            countdownText.text = $"Starting in {ceilSeconds}...";
            float scale = 1f + (ceilSeconds - secondsLeft) * 0.15f;
            countdownText.transform.localScale = new Vector3(scale, scale, 1f);
        }

        if (readyStatusText != null)
        {
            readyStatusText.text = $"Ready: {readyCount}/{totalCount}";
        }
    }

    public void HideCountdown()
    {
        if (countdownContainer != null)
        {
            countdownContainer.SetActive(false);
        }
    }
}
