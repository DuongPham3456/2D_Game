using UnityEngine;

// Bank: press E near it (via the ActionTrigger on this object) to pay off the entire
// tuition debt in one go. PayTuition guards affordability — if you can't cover the
// full debt yet, it just shows "not enough money" and nothing is charged.
[RequireComponent(typeof(ActionTrigger))]
public class BankPanel : MonoBehaviour
{
    PlayerStats stats;

    void Awake()
    {
        stats = FindFirstObjectByType<PlayerStats>();
        var trigger = GetComponent<ActionTrigger>();
        if (trigger != null) trigger.Interacted += PayAll;
    }

    void PayAll()
    {
        if (stats != null) stats.PayTuition(stats.TotalDebt);
    }
}
