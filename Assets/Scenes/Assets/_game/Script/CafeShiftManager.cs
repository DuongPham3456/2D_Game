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
        if (Active) return;
        Active = true;
        StartCoroutine(ShiftRoutine());
    }

    IEnumerator ShiftRoutine()
    {
        if (timerText != null) timerText.gameObject.SetActive(true);

        float remaining = shiftSeconds;
        while (remaining > 0f)
        {
            if (timerText != null)
            {
                int m = Mathf.FloorToInt(remaining / 60f);
                int s = Mathf.FloorToInt(remaining % 60f);
                timerText.text = $"Shift: {m}:{s:00}";
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
        timeManager?.AdvanceSlot();   // the shift spends one slot
    }
}
