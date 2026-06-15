using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    [Header("Time")]
    public int day = 1;
    public float hour = 7f;
    public float minute = 0f;
    public float timeSpeed = 10f;

    [Header("UI")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayText;

    [Header("References")]
    [SerializeField] PlayerStats playerStats;

    void Awake()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
    }

    void Update()
    {
        minute += timeSpeed * Time.deltaTime;

        while (minute >= 60f)
        {
            hour += 1f;
            minute -= 60f;
        }

        if (hour >= 24f)
            EndDay();

        UpdateUI();
    }

    void EndDay()
    {
        hour = 7f;
        minute = 0f;
        day++;

        playerStats?.OnNewDay();
        Debug.Log($"A new day at HUST — Day {day}");
    }

    void UpdateUI()
    {
        if (timeText != null)
            timeText.text = $"{(int)hour:00}:{(int)minute:00}";

        if (dayText != null)
            dayText.text = $"Day {day}";
    }

    public void SkipTime(float hoursToSkip)
    {
        hour += hoursToSkip;

        while (hour >= 24f)
        {
            hour -= 24f;
            day++;
            playerStats?.OnNewDay();
        }

        UpdateUI();
    }
}
