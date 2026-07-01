using UnityEngine;
using TMPro;

public enum DaySlot { Morning, Afternoon, Evening }

[System.Serializable]
public class FixedDailyEvent
{
    public string eventDescription;
    public int moneyChange;
    public float energyChange;
}

public class TimeManager : MonoBehaviour
{
    [Header("Time")]
    public int day = 1;

    [Header("Semester")]
    public int semesterDays = 10;

    [Header("Daily Fixed Events (index 0 = Day 1, leave description empty to skip)")]
    public FixedDailyEvent[] morningEvents = new FixedDailyEvent[10];

    [Header("UI")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI dayTimeText;   // optional combined label: "Day 3 – Morning"

    [Header("References")]
    [SerializeField] PlayerStats playerStats;
    [SerializeField] EndingManager endingManager;

    public DaySlot CurrentSlot { get; private set; } = DaySlot.Morning;

    DaySlot _shownSlot = (DaySlot)(-1);
    int _shownDay = -1;

    void Awake()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
        if (endingManager == null)
            endingManager = FindFirstObjectByType<EndingManager>();
    }

    void Start()
    {
        // Cap the framerate so it stops oscillating (the editor runs uncapped).
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        UpdateUI();
    }

    public void AdvanceSlot()
    {
        if (CurrentSlot == DaySlot.Morning)
            CurrentSlot = DaySlot.Afternoon;
        else if (CurrentSlot == DaySlot.Afternoon)
            CurrentSlot = DaySlot.Evening;
        // Evening: intentional no-op — player stays until they Sleep
        UpdateUI();
    }

    public void EndDay()
    {
        // Sleeping on the final day ends the semester instead of starting a new day.
        if (day >= semesterDays)
        {
            endingManager?.ShowEnding();
            return;
        }

        CurrentSlot = DaySlot.Morning;
        day++;
        playerStats?.OnNewDay();
        TriggerMorningEvent(day);
        Debug.Log($"A new day at HUST — Day {day}");
        UpdateUI();
    }

    // Fires the scripted event for this day (if one is set), applying its
    // money/energy change and showing a popup.
    void TriggerMorningEvent(int currentDay)
    {
        int index = currentDay - 1;
        if (index < 0 || index >= morningEvents.Length) return;

        FixedDailyEvent evt = morningEvents[index];
        if (evt == null || string.IsNullOrEmpty(evt.eventDescription)) return;

        Debug.Log($"Day {currentDay} event: {evt.eventDescription}");
        playerStats?.ApplyDailyEvent(evt.moneyChange, evt.energyChange, evt.eventDescription);
        NotificationManager.Instance?.Show(evt.eventDescription, 4f);
    }

    void UpdateUI()
    {
        if (timeText != null && CurrentSlot != _shownSlot)
        {
            timeText.text = CurrentSlot.ToString();
            _shownSlot = CurrentSlot;
        }

        if (dayText != null && day != _shownDay)
        {
            dayText.text = $"Day {day}";
            _shownDay = day;
        }

        if (dayTimeText != null)
            dayTimeText.text = $"Day: {day} - {CurrentSlot}";
    }
}
