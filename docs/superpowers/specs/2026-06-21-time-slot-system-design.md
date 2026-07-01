# Time Slot System Design

**Date:** 2026-06-21  
**Status:** Approved

---

## Context

The original `TimeManager` used a real-time clock (`minute += timeSpeed * Time.deltaTime`) where activities called `SkipTime(hours)` to jump the clock forward. This is being replaced with a discrete slot-based system that better fits the game's turn-style activity loop.

---

## Design

### Slot Progression

Each day has 3 ordered slots:

```
Morning → Afternoon → Evening → (Rest) → Morning (Day + 1)
```

- **Morning / Afternoon:** The first activity the player completes advances the slot. Only one activity is consumed per slot.
- **Evening:** Any number of activities can be done. The player stays in Evening until they choose to Rest, which ends the day.
- All activities (Study, Work, Rest) are available in every slot.

---

### Data Model

A `DaySlot` enum replaces `hour` and `minute`:

```csharp
public enum DaySlot { Morning, Afternoon, Evening }
```

`TimeManager` fields:
- `DaySlot currentSlot` — starts at `Morning`
- `int day` — unchanged, starts at 1

Removed fields: `hour`, `minute`, `timeSpeed`

---

### TimeManager

`Update()` is removed — no real-time clock.

**Public API:**

| Method | Behaviour |
|---|---|
| `AdvanceSlot()` | Morning→Afternoon, Afternoon→Evening, Evening→no-op |
| `EndDay()` | currentSlot = Morning, day++, fires `playerStats?.OnNewDay()`, calls `UpdateUI()` |
| `DaySlot CurrentSlot` | Read-only property for `PlayerStats` to check before Rest |

**UpdateUI():** maps slot to display string and updates `timeText`. Caches `_shownSlot` so TMP mesh only rebuilds on actual change. `dayText` keeps `"Day X"` format (unchanged).

| Slot | timeText |
|---|---|
| Morning | "Morning" |
| Afternoon | "Afternoon" |
| Evening | "Evening" |

---

### PlayerStats

`AdvanceTime(float hours)` is removed.

| Method | Time call |
|---|---|
| `Study()` | `timeManager.AdvanceSlot()` |
| `Work()` | `timeManager.AdvanceSlot()` |
| `Rest()` | If `currentSlot == Evening` → `timeManager.EndDay()`, else → `timeManager.AdvanceSlot()` |

All stat logic (stamina costs, GPA gain, money, UI caching) is unchanged.

---

### Removed

- `TimeManager.hour`, `minute`, `timeSpeed`
- `TimeManager.Update()`
- `TimeManager.SkipTime(float hours)`
- `PlayerStats.AdvanceTime(float hours)`
- `TimeManager._shownHour`, `_shownMinute` cache fields

---

## Verification

1. Enter Play Mode. UI should show "Morning" and "Day 1".
2. Trigger Study or Work — UI should advance to "Afternoon".
3. Trigger Study or Work again — UI should advance to "Evening".
4. Trigger Study or Work in Evening — slot stays "Evening".
5. Trigger Rest in Evening — slot resets to "Morning", day increments to "Day 2".
6. Trigger Rest in Morning — slot advances to "Afternoon" (does not end the day).
7. Confirm stamina, GPA, money, and debt still update correctly after each action.
