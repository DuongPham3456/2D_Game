using UnityEngine;
using TMPro;

public enum DaySlot { Morning, Afternoon, Evening }

public class TimeManager : MonoBehaviour
{
    [Header("Time")]
    public int day = 1;

    [Header("Semester")]
    public int semesterDays = 10;

    [Header("UI")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayText;

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
        Debug.Log($"A new day at HUST — Day {day}");
        UpdateUI();
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
    }
}
