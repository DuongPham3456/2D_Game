using UnityEngine;
using UnityEngine.UI;

// Swaps a background image to match the current time slot (Morning/Afternoon/Evening),
// read from the slot-based TimeManager. Works with a world SpriteRenderer OR a UI Image
// (assign whichever you use). Purely visual — it follows time, doesn't drive it.
public class TimeOfDayBackground : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TimeManager timeManager;
    [SerializeField] SpriteRenderer background;    // world background (leave null if UI)
    [SerializeField] Image backgroundImage;        // UI/canvas background (leave null if world)

    [Header("Sprite per slot")]
    [SerializeField] Sprite morning;
    [SerializeField] Sprite afternoon;
    [SerializeField] Sprite evening;

    DaySlot _shown = (DaySlot)(-1);

    void Awake()
    {
        if (timeManager == null) timeManager = FindFirstObjectByType<TimeManager>();
    }

    void Update()
    {
        // Only swap when the slot actually changes (cheap poll, no per-frame work).
        if (timeManager == null || timeManager.CurrentSlot == _shown) return;
        _shown = timeManager.CurrentSlot;

        Sprite s = SpriteForSlot();
        if (background != null) background.sprite = s;
        if (backgroundImage != null) backgroundImage.sprite = s;
    }

    Sprite SpriteForSlot()
    {
        switch (timeManager.CurrentSlot)
        {
            case DaySlot.Morning:   return morning;
            case DaySlot.Afternoon: return afternoon;
            default:                return evening;   // Evening
        }
    }
}
