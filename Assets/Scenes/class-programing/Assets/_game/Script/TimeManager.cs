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

    [Header("Daily Fixed Events")]
    [Tooltip("Điền 10 sự kiện. Element 0 = Ngày 1, Element 1 = Ngày 2...")]
    public FixedDailyEvent[] morningEvents = new FixedDailyEvent[10];

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
        
        UpdateUI();
    }

    public void EndDay()
    {
        if (day >= semesterDays)
        {
            endingManager?.ShowEnding();
            return;
        }

        CurrentSlot = DaySlot.Morning;
        day++;
        
        // Trừ tiền sinh hoạt mỗi ngày
        playerStats?.OnNewDay();
        
        // Bắn sự kiện ngẫu nhiên
        TriggerMorningEvent(day);

        Debug.Log($"A new day at HUST — Day {day}");
        UpdateUI();
    }

    void TriggerMorningEvent(int currentDay)
    {
        int index = currentDay - 1; 

        if (index >= 0 && index < morningEvents.Length)
        {
            FixedDailyEvent evt = morningEvents[index];

            if (!string.IsNullOrEmpty(evt.eventDescription))
            {
                Debug.Log($"SỰ KIỆN NGÀY {currentDay}: {evt.eventDescription}");

                if (playerStats != null)
                {
                    playerStats.ApplyDailyEvent(evt.moneyChange, evt.energyChange, evt.eventDescription);
                }

                if (NotificationManager.Instance != null) 
                {
                    NotificationManager.Instance.HienThongBao(evt.eventDescription, 4f);
                }
            }
        }
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