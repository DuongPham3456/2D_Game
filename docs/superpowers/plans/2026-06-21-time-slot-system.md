# Time Slot System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the real-time clock in TimeManager with a discrete Morning / Afternoon / Evening slot system where completing one activity advances the slot.

**Architecture:** `DaySlot` enum lives in `TimeManager.cs`. `TimeManager` exposes `AdvanceSlot()` and `EndDay()` instead of `SkipTime()`. `PlayerStats` calls those two methods from `Study()`, `Work()`, and `Rest()` — no other files change.

**Tech Stack:** Unity 2D, C#, TextMeshPro (TMP), New Input System

## Global Constraints

- Target files are under `Assets/Scenes/class-programing/Assets/_game/Script/`
- No automated test runner is configured — verification is done in Unity Play Mode
- Do NOT add new GameObjects, prefabs, scenes, or packages
- Keep all existing `[SerializeField]` and `[Header]` attributes intact
- `DaySlot` enum must be `public` so `PlayerStats` can reference it without adding `using` directives

---

## File Map

| File | Change |
|---|---|
| `TimeManager.cs` | Major rewrite — remove clock fields, add enum + slot API |
| `PlayerStats.cs` | Remove `AdvanceTime()`, update `Study()` / `Work()` / `Rest()` |

---

### Task 1: Rewrite TimeManager.cs

**Files:**
- Modify: `Assets/Scenes/class-programing/Assets/_game/Script/TimeManager.cs`

**Interfaces:**
- Produces:
  - `public enum DaySlot { Morning, Afternoon, Evening }` (top-level, same file)
  - `public DaySlot CurrentSlot { get; private set; }`
  - `public void AdvanceSlot()`
  - `public void EndDay()`

---

- [ ] **Step 1: Replace the full contents of TimeManager.cs**

Open `Assets/Scenes/class-programing/Assets/_game/Script/TimeManager.cs` and replace everything with:

```csharp
using UnityEngine;
using TMPro;

public enum DaySlot { Morning, Afternoon, Evening }

public class TimeManager : MonoBehaviour
{
    [Header("Time")]
    public int day = 1;

    [Header("UI")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayText;

    [Header("References")]
    [SerializeField] PlayerStats playerStats;

    public DaySlot CurrentSlot { get; private set; } = DaySlot.Morning;

    DaySlot _shownSlot = (DaySlot)(-1);
    int _shownDay = -1;

    void Awake()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
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
        // Evening: intentional no-op — player stays until they Rest
        UpdateUI();
    }

    public void EndDay()
    {
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
```

- [ ] **Step 2: Verify the project still compiles**

In Unity, check the Console window (Window → General → Console). There should be **zero** compile errors. If you see errors about `SkipTime` or `hour`/`minute` — those will be fixed in Task 2.

- [ ] **Step 3: Commit**

```bash
git add Assets/Scenes/class-programing/Assets/_game/Script/TimeManager.cs
git commit -m "feat: replace real-time clock with Morning/Afternoon/Evening slot system"
```

---

### Task 2: Update PlayerStats.cs

**Files:**
- Modify: `Assets/Scenes/class-programing/Assets/_game/Script/PlayerStats.cs`

**Interfaces:**
- Consumes:
  - `timeManager.AdvanceSlot()` — `public void`, defined in Task 1
  - `timeManager.EndDay()` — `public void`, defined in Task 1
  - `timeManager.CurrentSlot` — `public DaySlot`, defined in Task 1
  - `DaySlot.Evening` — from `public enum DaySlot`, defined in Task 1

---

- [ ] **Step 1: Remove the AdvanceTime method**

Find and delete this entire method (around line 161):

```csharp
void AdvanceTime(float hours)
{
    if (timeManager != null)
        timeManager.SkipTime(hours);
}
```

- [ ] **Step 2: Update Study()**

Replace the existing `Study()` method with:

```csharp
public void Study()
{
    if (!HasStamina(studyStaminaCost, "study"))
        return;

    stamina -= studyStaminaCost;
    gpa = Mathf.Min(maxGpa, gpa + studyGpaGain);
    timeManager?.AdvanceSlot();
    ShowMessage($"Studied at HUST library. GPA +{studyGpaGain:F1}");
    UpdateUI();
}
```

- [ ] **Step 3: Update Work()**

Replace the existing `Work()` method with:

```csharp
public void Work()
{
    if (!HasStamina(workStaminaCost, "work a cafe shift"))
        return;

    stamina -= workStaminaCost;
    money += workMoneyGain;
    timeManager?.AdvanceSlot();
    ShowMessage($"Cafe shift done. +{workMoneyGain:N0} VND");
    UpdateUI();
}
```

- [ ] **Step 4: Update Rest()**

Replace the existing `Rest()` method with:

```csharp
public void Rest()
{
    if (restSetsStaminaToMax)
        stamina = maxStamina;
    else
        stamina = Mathf.Min(maxStamina, stamina + restStaminaRestore);

    if (timeManager != null && timeManager.CurrentSlot == DaySlot.Evening)
        timeManager.EndDay();
    else
        timeManager?.AdvanceSlot();

    ShowMessage("Rested at the dorm. Stamina restored.");
    UpdateUI();
}
```

- [ ] **Step 5: Verify zero compile errors in Unity Console**

All references to `SkipTime` and `AdvanceTime` should now be gone. Console should be clean.

- [ ] **Step 6: Commit**

```bash
git add Assets/Scenes/class-programing/Assets/_game/Script/PlayerStats.cs
git commit -m "feat: update PlayerStats to use slot-based time advancement"
```

---

### Task 3: Play Mode Verification

**Files:** None — verification only.

- [ ] **Step 1: Enter Play Mode in Unity**

Press the Play button. The `timeText` UI element should display **"Morning"** and `dayText` should display **"Day 1"**.

- [ ] **Step 2: Verify Morning → Afternoon**

Trigger Study or Work (via an ActionTrigger zone, pressing E). The `timeText` should update to **"Afternoon"**. Day stays at **"Day 1"**.

- [ ] **Step 3: Verify Afternoon → Evening**

Trigger Study or Work again. The `timeText` should update to **"Evening"**. Day stays at **"Day 1"**.

- [ ] **Step 4: Verify Evening stays in Evening**

Trigger Study or Work while in Evening. The `timeText` should stay **"Evening"**. Day stays at **"Day 1"**.

- [ ] **Step 5: Verify Rest in Evening ends the day**

Trigger Rest while in Evening. The `timeText` should reset to **"Morning"** and `dayText` should update to **"Day 2"**. The Console should log `"A new day at HUST — Day 2"`. `OnNewDay` fires (money decrements by `dailyLivingCost`).

- [ ] **Step 6: Verify Rest outside Evening advances slot**

Exit Play Mode, re-enter. Trigger Rest while in **Morning**. The `timeText` should advance to **"Afternoon"** — it should NOT jump to Day 2.

- [ ] **Step 7: Verify stats still work**

Confirm GPA increments after Study, money increments after Work, stamina restores after Rest, and the stat UI labels update correctly.

- [ ] **Step 8: Exit Play Mode**
