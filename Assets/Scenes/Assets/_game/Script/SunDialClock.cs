using UnityEngine;
using UnityEngine.UI;

// Cosmetic sun-dial gauge. A needle sweeps a half-dome to reflect the current time
// slot (Morning/Afternoon/Evening) read from the game's slot-based TimeManager.
// Purely visual — it does not drive time; it follows it.
public class SunDialClock : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TimeManager timeManager;
    [SerializeField] RectTransform needle;   // the sun/needle that rotates over the dome

    [Header("Needle angle per slot (Z degrees) — tune to your dome art")]
    [SerializeField] float morningAngle = 90f;    // sunrise, left horizon
    [SerializeField] float afternoonAngle = 0f;   // noon, top
    [SerializeField] float eveningAngle = -90f;   // sunset, right horizon

    [Header("Sweep speed (deg/sec toward the target slot)")]
    [SerializeField] float sweepSpeed = 90f;

    [Header("Sky fill (optional) — tints day -> night")]
    [SerializeField] Image sky;
    [SerializeField] Color morningColor   = new Color(1f, 0.85f, 0.5f);   // warm sunrise
    [SerializeField] Color afternoonColor = new Color(0.5f, 0.8f, 1f);    // bright noon
    [SerializeField] Color eveningColor   = new Color(0.15f, 0.15f, 0.35f); // dark dusk
    [SerializeField] float tintSpeed = 1f;   // color units/sec toward the target

    void Awake()
    {
        if (timeManager == null) timeManager = FindFirstObjectByType<TimeManager>();
    }

    void Start()
    {
        // Snap to the starting slot so it doesn't sweep/fade in from wherever on load.
        if (needle != null)
            needle.localRotation = Quaternion.Euler(0f, 0f, AngleForSlot());
        if (sky != null)
            sky.color = ColorForSlot();
    }

    void Update()
    {
        if (timeManager == null) return;

        if (needle != null)
        {
            // MoveTowardsAngle takes the shortest path, so the day-reset (Evening -> Morning)
            // sweeps back the short way instead of spinning through the whole arc.
            float z = Mathf.MoveTowardsAngle(
                needle.localEulerAngles.z, AngleForSlot(), sweepSpeed * Time.deltaTime);
            needle.localRotation = Quaternion.Euler(0f, 0f, z);
        }

        if (sky != null)
            sky.color = Vector4.MoveTowards(sky.color, ColorForSlot(), tintSpeed * Time.deltaTime);
    }

    float AngleForSlot()
    {
        switch (timeManager.CurrentSlot)
        {
            case DaySlot.Morning:   return morningAngle;
            case DaySlot.Afternoon: return afternoonAngle;
            default:                return eveningAngle;   // Evening
        }
    }

    Color ColorForSlot()
    {
        switch (timeManager.CurrentSlot)
        {
            case DaySlot.Morning:   return morningColor;
            case DaySlot.Afternoon: return afternoonColor;
            default:                return eveningColor;   // Evening
        }
    }
}
