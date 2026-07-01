using System.Collections;
using UnityEngine;
using TMPro;

// One cafe shift = one time slot (like Work / the exam).
// Clock in via an ActionTrigger -> StartShift(), play the coffee minigame for
// shiftSeconds, then the slot advances automatically. Counters only pay while Active.
public class CafeShiftManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TimeManager timeManager;
    [SerializeField] PlayerStats playerStats;

    [Header("Shift")]
    [SerializeField] float shiftSeconds = 300f;   // 5 minutes
    [SerializeField] TextMeshProUGUI timerText;   // optional HUD, hidden when off shift

    public bool Active { get; private set; }

    void Awake()
    {
        if (timeManager == null) timeManager = FindFirstObjectByType<TimeManager>();
        if (playerStats == null) playerStats = FindFirstObjectByType<PlayerStats>();
    }

    void Start()
    {
        if (timerText != null) timerText.gameObject.SetActive(false);
    }

    // Wire a cafe ActionTrigger's onInteract to this.
    public void StartShift()
    {
        if (Active)
        {
            Debug.Log("[Cafe] Already on a shift — can't clock in again.");
            return;
        }
        Active = true;
        Debug.Log($"[Cafe] Clocked in! Shift started ({FormatTime(shiftSeconds)}). Press C at the counters to make coffee.");
        StartCoroutine(ShiftRoutine());
    }

    IEnumerator ShiftRoutine()
    {
        if (timerText != null) timerText.gameObject.SetActive(true);

        int startDay = timeManager != null ? timeManager.day : 0;
        float remaining = shiftSeconds;
        int lastLoggedSecond = -1;
        while (remaining > 0f)
        {
            // Slept / day changed mid-shift — abandon it without spending a slot.
            if (timeManager != null && timeManager.day != startDay)
            {
                Active = false;
                if (timerText != null) timerText.gameObject.SetActive(false);
                Debug.Log("[Cafe] Shift abandoned (day changed) — no time slot spent.");
                yield break;
            }

            int sec = Mathf.CeilToInt(remaining);
            if (timerText != null) timerText.text = $"Shift: {FormatTime(sec)}";

            // Console fallback while there's no HUD: log each minute mark + final 10s.
            if (sec != lastLoggedSecond && (sec % 60 == 0 || sec <= 10))
            {
                Debug.Log($"[Cafe] Time left: {FormatTime(sec)}");
                lastLoggedSecond = sec;
            }

            remaining -= Time.deltaTime;
            yield return null;
        }

        EndShift();
    }

    void EndShift()
    {
        Active = false;
        if (timerText != null) timerText.gameObject.SetActive(false);
        Debug.Log("[Cafe] Shift over — pay collected. Time slot advanced.");
        timeManager?.AdvanceSlot();   // the shift spends one slot
    }

    static string FormatTime(float seconds)
    {
        int total = Mathf.Max(0, Mathf.CeilToInt(seconds));
        return $"{total / 60}:{total % 60:00}";
    }
}