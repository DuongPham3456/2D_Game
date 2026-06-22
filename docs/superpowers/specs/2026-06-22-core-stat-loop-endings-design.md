# Core Stat / Loop / Endings — Design

**Date:** 2026-06-22
**Status:** Draft — awaiting user review

---

## Context

The game is a student-life sim at HUST. The current code (`PlayerStats`) tracks
`gpa`, `money`, `stamina`, `totalDebt` and exposes `Study()`, `Work()`, `Rest()`;
`TimeManager` runs a Morning/Afternoon/Evening slot system. This spec redefines the
**core loop**: the stat model, how each activity moves the stats, what ends the
game, and which ending the player gets.

**Out of scope (separate later specs):**
- The 5-minute Work check-in mini-game — this spec only needs "Work consumes a
  slot and returns money."
- The detailed Study mechanic and random trigger events.
- Food/inventory economy.

---

## Stats

Four meters. Energy and Sanity are 0–100; Knowledge is 0–200; Money is uncapped
(and can go negative via debt). All starting values are placeholders for balancing.

| Stat | Range | Start | Good dir. | Role |
|---|---|---|---|---|
| Energy | 0–100 | 100 | high | Gates activity. Only Sleep restores it (partial). |
| Sanity *(UI label: "Insanity")* | 0–100 | 100 | high | Drains from effortful activity, restored by Entertain. Low = debuff. |
| Knowledge | 0–200 | 0 | high | Accumulates all semester; converts to final GPA. |
| Money | uncapped | start cash, owe `totalDebt` | high | Earned by Work, spent paying debt. |

`GPA` is **not** a directly-mutated stat anymore. It is **derived from Knowledge**
at the end of the game (see Endings).

---

## Time & Semester

- The slot system (Morning → Afternoon → Evening) is retained from the existing
  `TimeManager`. Each activity consumes a slot per the existing rules.
- The semester has a **fixed length** of `semesterDays` days (placeholder: 30).
- **Sleep** ends the current day. When the day counter passes `semesterDays`, the
  game ends and the ending is evaluated.

---

## Activities

Each activity consumes one timeslot. Effect numbers are placeholders.

| Place / Action | Energy | Sanity | Knowledge | Money | Notes |
|---|---|---|---|---|---|
| Classroom — Study | −15 | −10 | +12 | — | strong knowledge, heavy cost |
| Cafe — Work | −20 | −8 | — | +pay | pay returned by mini-game later; v1 uses a flat amount |
| Bedroom PC — Study | −10 | −5 | +6 | — | gentle grind |
| Bedroom PC — Entertain | −2 | +15 | — | — | the only Sanity recovery; earns nothing |
| Sleep (ends day) | +partial | +small | — | — | not slot-gated; advances the day |

**Energy gate:** if Energy is below an activity's Energy cost, that activity is
**unavailable** (button greyed / interaction refused). Entertain's cost is low
enough that the player is never fully soft-locked — they can always Entertain or
Sleep.

**Paying debt:** the player can pay money toward `totalDebt` (existing
`PayTuition` flow). Money spent on debt is gone; this is the tension against any
future self-spending.

---

## Sanity Debuff (two stages)

- **Stressed — Sanity < 25:** Knowledge gains are halved (×0.5) and Energy costs
  are increased (×1.25) on every activity. The player can still act, but
  inefficiently — a nudge to go Entertain.
- **Breakdown — Sanity reaches 0:** the player's **next chosen action is wasted**
  (its slot is consumed, no stat changes applied), then Sanity is set back to
  **15**. Sharp, memorable, never soft-locks. No game-over from Sanity in v1.

Stressed is a continuous modifier; Breakdown is a one-shot triggered when Sanity
hits 0 and cleared after the next action is burned.

---

## Endings

Evaluated once, when the semester ends.

**Knowledge → GPA** (linear, GPA = round(Knowledge / 200 × 4.0, 1)):

| Knowledge | GPA | Class |
|---|---|---|
| 0–49 | < 2.0 | fail |
| 50–99 | 2.0–2.9 | average |
| 100–149 | 3.0–3.5 | good |
| 150–200 | 3.6–4.0 | excellent |

**Ending = GPA bracket × Debt status** (high GPA = GPA ≥ 3.0; debt cleared =
`totalDebt <= 0`):

| | Debt cleared | Debt remaining |
|---|---|---|
| High GPA | **Honors Graduate** (best) | **Brilliant but Broke** |
| Low GPA | **Free but Adrift** | **Crushed** (worst) |

A 5th secret ending (e.g. GPA 4.0 + debt cleared + Sanity never bottomed) is
noted as a possible later addition, not in v1.

---

## Component Mapping

How this lands on the codebase. Detailed file/method breakdown is the
implementation plan's job; this is the architecture.

- **`PlayerStats`** — holds the four stats. `stamina`→`energy`; add `sanity`,
  `knowledge`. Remove direct `gpa` mutation and `studyGpaGain`. Rework
  `Study()`/`Work()`; add `StudyPC()`, `EntertainPC()`. Apply the Stressed
  modifier and Breakdown rule inside the activity path. Expose a `Gpa` read-only
  computed from `knowledge`.
- **`TimeManager`** — already owns slots and `day`. Add `semesterDays` and an
  end-of-semester check on `EndDay()` that, when the semester is over, calls the
  ending evaluator instead of starting a new day. Sleep maps to the existing
  end-day path plus the partial Energy/Sanity restore.
- **Ending evaluation** — a small `EndingManager` (or method on `TimeManager`)
  that reads `knowledge` → GPA and `totalDebt`, picks the 2×2 ending, and shows
  it. Kept separate so endings logic is testable on its own.
- **`StudentGameConfig`** — update the ScriptableObject fields to the new stat
  set and per-activity costs/gains, so balancing stays data-driven.
- **`ActionTrigger`** — gains the new station types (PC-Study, Entertain) and
  respects the Energy gate when showing/allowing an interaction.

---

## v1 Simplifications (deliberate)

- Work pay is a flat config value; the mini-game replaces it later behind the
  same "Work → money" interface.
- No food/items/inventory; Sleep is the only Energy restore.
- No random study events yet; the Study path leaves a clean hook for them.
- One secret ending deferred.

---

## Verification (Play Mode)

1. Study in the classroom: Knowledge up, Energy and Sanity down, one slot consumed.
2. Drive Sanity below 25: confirm Knowledge gains halve and Energy costs rise.
3. Drive Sanity to 0: next action is wasted (slot gone, no gains), Sanity returns to 15.
4. Energy below an activity's cost: that activity is unavailable; Entertain/Sleep still work.
5. Sleep: day advances, Energy/Sanity partially restore.
6. Reach the final day: the correct ending fires for the current Knowledge→GPA and debt status — spot-check all four 2×2 cells.
