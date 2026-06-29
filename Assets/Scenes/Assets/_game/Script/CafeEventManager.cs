using UnityEngine;

// Turns cafe NPCs on for their scripted day. Assign the NPC objects (left disabled).
// Re-checks when the day changes, so it works in a single persistent scene.
public class CafeEventManager : MonoBehaviour
{
    [SerializeField] TimeManager timeManager;

    [Header("NPCs (kept off until their day)")]
    public GameObject npcKid;          // Day 3: bumps the player (costs money)
    public GameObject npcDineAndDash;  // Day 1: runs off without paying (costs energy)

    int _lastDay = -1;

    void Start()
    {
        if (timeManager == null) timeManager = FindFirstObjectByType<TimeManager>();
        if (npcKid != null) npcKid.SetActive(false);
        if (npcDineAndDash != null) npcDineAndDash.SetActive(false);
    }

    void Update()
    {
        if (timeManager == null || timeManager.day == _lastDay) return;
        _lastDay = timeManager.day;
        Evaluate(_lastDay);
    }

    void Evaluate(int day)
    {
        if (day == 1 && npcDineAndDash != null) npcDineAndDash.SetActive(true);
        else if (day == 3 && npcKid != null) npcKid.SetActive(true);
    }
}
