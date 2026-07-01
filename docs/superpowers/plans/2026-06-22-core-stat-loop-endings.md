# Core Stat / Loop / Endings Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the GPA/stamina stat model with Energy + Sanity + Knowledge (+ Money), a 10-day semester, a Knowledge→GPA × Debt endings matrix, and a two-stage low-Sanity debuff.

**Architecture:** Pure scoring/ending logic lives in a static `GameRules` class in its own assembly (`HUST.Core`) so it can be unit-tested with NUnit. `PlayerStats` holds the four stats and runs each activity through `GameRules` for the debuff math. `TimeManager` owns the semester clock and, on the final day, hands off to `EndingManager`, which reads the stats, asks `GameRules` for the ending, and shows it.

**Tech Stack:** Unity 2D, C#, TextMeshPro, New Input System, Unity Test Framework (NUnit, EditMode).

## Global Constraints

- Target gameplay scripts live under `Assets/Scenes/class-programing/Assets/_game/Script/`.
- Stat ranges: **Energy 0–100, Sanity 0–100, Knowledge 0–200**; Money is uncapped and may go negative.
- Sanity is labelled **"Insanity"** in on-screen UI text but high = healthy.
- Knowledge class thresholds (exact): **excellent ≥ 180, good ≥ 130, average ≥ 70, fail < 70**.
- Endings **High GPA row = excellent only** (Knowledge ≥ 180); debt cleared = `totalDebt <= 0`.
- Sanity debuff: **Stressed when Sanity < 25** (Knowledge gain ×0.5, Energy cost ×1.25); **Breakdown at Sanity 0** (next action wasted, Sanity set to 15).
- Semester length = **10 days**; Sleep is the only Energy restore (partial).
- Each activity consumes one timeslot via the existing `TimeManager.AdvanceSlot()` / `EndDay()` slot system. Do not change the slot progression.
- Out of scope (later specs): the Work check-in mini-game (use a flat money value), random study events, food/inventory.

---

## File Map

| File | Change |
|---|---|
| `Script/Core/GameRules.cs` | **Create** — pure static logic: class/GPA, ending selection, debuff multipliers |
| `Script/Core/HUST.Core.asmdef` | **Create** — assembly for GameRules so tests can reference it |
| `Script/Tests/Editor/HUST.Core.Tests.asmdef` | **Create** — EditMode test assembly |
| `Script/Tests/Editor/GameRulesTests.cs` | **Create** — NUnit tests for GameRules |
| `Script/StudentGameConfig.cs` | **Rewrite** — new stat/cost/gain fields |
| `Script/PlayerStats.cs` | **Rewrite** — four stats, activities, debuff, computed GPA |
| `Script/EndingManager.cs` | **Create** — evaluate + display ending |
| `Script/TimeManager.cs` | **Modify** — semester end → EndingManager |
| `Script/ActionTrigger.cs` | **Modify** — new station types + prompts |

Path prefix for all of the above: `Assets/Scenes/class-programing/Assets/_game/Script/`.

**Scene rewiring (manual, after code):** the redesign renames stats and adds activities, so existing UI text bindings and `ActionTrigger.onInteract` events must be re-pointed in the Unity Inspector. This is called out in the final task's verification, not automated.

---

### Task 1: GameRules pure logic + tests

**Files:**
- Create: `Script/Core/GameRules.cs`
- Create: `Script/Core/HUST.Core.asmdef`
- Create: `Script/Tests/Editor/HUST.Core.Tests.asmdef`
- Create: `Script/Tests/Editor/GameRulesTests.cs`

**Interfaces:**
- Produces (all `static`, on `public static class GameRules`):
  - `enum KnowledgeClass { Fail, Average, Good, Excellent }`
  - `enum Ending { HonorsGraduate, BrilliantButBroke, FreeButAdrift, Crushed }`
  - `const float ExcellentThreshold = 180f`, `GoodThreshold = 130f`, `AverageThreshold = 70f`
  - `const float StressedThreshold = 25f`, `BreakdownRecoverTo = 15f`
  - `KnowledgeClass ClassFor(float knowledge)`
  - `float GpaFor(float knowledge)` — Knowledge/200×4, clamped 0–200, rounded to 1 decimal
  - `Ending SelectEnding(float knowledge, int debt)`
  - `string TitleOf(Ending e)`
  - `float StressedKnowledgeMultiplier(float sanity)` — 0.5 if Stressed else 1
  - `float StressedEnergyMultiplier(float sanity)` — 1.25 if Stressed else 1

- [ ] **Step 1: Create the GameRules assembly definition**

Create `Script/Core/HUST.Core.asmdef`:

```json
{
    "name": "HUST.Core",
    "rootNamespace": "",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

`autoReferenced: true` means the default `Assembly-CSharp` (where `PlayerStats` etc. live) automatically sees `GameRules` with no extra wiring.

- [ ] **Step 2: Write GameRules**

Create `Script/Core/GameRules.cs`:

```csharp
using UnityEngine;

/// Pure, engine-light game scoring rules. No MonoBehaviour, no scene state —
/// everything here is deterministic and unit-tested.
public static class GameRules
{
    public enum KnowledgeClass { Fail, Average, Good, Excellent }
    public enum Ending { HonorsGraduate, BrilliantButBroke, FreeButAdrift, Crushed }

    public const float ExcellentThreshold = 180f;
    public const float GoodThreshold = 130f;
    public const float AverageThreshold = 70f;

    public const float StressedThreshold = 25f;
    public const float BreakdownRecoverTo = 15f;

    public static KnowledgeClass ClassFor(float knowledge)
    {
        if (knowledge >= ExcellentThreshold) return KnowledgeClass.Excellent;
        if (knowledge >= GoodThreshold) return KnowledgeClass.Good;
        if (knowledge >= AverageThreshold) return KnowledgeClass.Average;
        return KnowledgeClass.Fail;
    }

    public static float GpaFor(float knowledge)
    {
        float gpa = Mathf.Clamp(knowledge, 0f, 200f) / 200f * 4f;
        return Mathf.Round(gpa * 10f) / 10f;
    }

    public static Ending SelectEnding(float knowledge, int debt)
    {
        bool highGpa = knowledge >= ExcellentThreshold;
        bool debtCleared = debt <= 0;
        if (highGpa) return debtCleared ? Ending.HonorsGraduate : Ending.BrilliantButBroke;
        return debtCleared ? Ending.FreeButAdrift : Ending.Crushed;
    }

    public static string TitleOf(Ending e)
    {
        switch (e)
        {
            case Ending.HonorsGraduate: return "Honors Graduate";
            case Ending.BrilliantButBroke: return "Brilliant but Broke";
            case Ending.FreeButAdrift: return "Free but Adrift";
            default: return "Crushed";
        }
    }

    public static float StressedKnowledgeMultiplier(float sanity)
        => sanity < StressedThreshold ? 0.5f : 1f;

    public static float StressedEnergyMultiplier(float sanity)
        => sanity < StressedThreshold ? 1.25f : 1f;
}
```

- [ ] **Step 3: Create the test assembly definition**

Create `Script/Tests/Editor/HUST.Core.Tests.asmdef`:

```json
{
    "name": "HUST.Core.Tests",
    "rootNamespace": "",
    "references": ["HUST.Core", "UnityEngine.TestRunner", "UnityEditor.TestRunner"],
    "includePlatforms": ["Editor"],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": ["nunit.framework.dll"],
    "autoReferenced": false,
    "defineConstraints": ["UNITY_INCLUDE_TESTS"],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 4: Write the failing tests**

Create `Script/Tests/Editor/GameRulesTests.cs`:

```csharp
using NUnit.Framework;

public class GameRulesTests
{
    [TestCase(69f, GameRules.KnowledgeClass.Fail)]
    [TestCase(70f, GameRules.KnowledgeClass.Average)]
    [TestCase(129f, GameRules.KnowledgeClass.Average)]
    [TestCase(130f, GameRules.KnowledgeClass.Good)]
    [TestCase(179f, GameRules.KnowledgeClass.Good)]
    [TestCase(180f, GameRules.KnowledgeClass.Excellent)]
    [TestCase(200f, GameRules.KnowledgeClass.Excellent)]
    public void ClassFor_RespectsThresholds(float knowledge, GameRules.KnowledgeClass expected)
    {
        Assert.AreEqual(expected, GameRules.ClassFor(knowledge));
    }

    [TestCase(0f, 0f)]
    [TestCase(100f, 2f)]
    [TestCase(180f, 3.6f)]
    [TestCase(200f, 4f)]
    public void GpaFor_IsLinearAndRounded(float knowledge, float expected)
    {
        Assert.AreEqual(expected, GameRules.GpaFor(knowledge), 0.001f);
    }

    [TestCase(180, 0, GameRules.Ending.HonorsGraduate)]
    [TestCase(180, 5, GameRules.Ending.BrilliantButBroke)]
    [TestCase(120, 0, GameRules.Ending.FreeButAdrift)]
    [TestCase(120, 5, GameRules.Ending.Crushed)]
    public void SelectEnding_PicksCorrectCell(float knowledge, int debt, GameRules.Ending expected)
    {
        Assert.AreEqual(expected, GameRules.SelectEnding(knowledge, debt));
    }

    [Test]
    public void StressedMultipliers_KickInBelow25()
    {
        Assert.AreEqual(1f, GameRules.StressedKnowledgeMultiplier(25f), 0.001f);
        Assert.AreEqual(0.5f, GameRules.StressedKnowledgeMultiplier(24f), 0.001f);
        Assert.AreEqual(1f, GameRules.StressedEnergyMultiplier(25f), 0.001f);
        Assert.AreEqual(1.25f, GameRules.StressedEnergyMultiplier(24f), 0.001f);
    }
}
```

- [ ] **Step 5: Run the tests**

In Unity: **Window → General → Test Runner → EditMode → Run All**. Expected: all `GameRulesTests` pass (4 ClassFor cases + 4 Gpa cases + 4 ending cases + 1 multiplier test). If the test assembly shows compile errors about `GameRules` not found, confirm `HUST.Core` appears in the Tests asmdef `references` and that both `.asmdef` files imported without error.

- [ ] **Step 6: Commit**

```bash
git add "Assets/Scenes/class-programing/Assets/_game/Script/Core" "Assets/Scenes/class-programing/Assets/_game/Script/Tests"
git commit -m "feat: add GameRules scoring/ending logic with EditMode tests"
```

---

### Task 2: StudentGameConfig new fields

**Files:**
- Modify (full rewrite): `Script/StudentGameConfig.cs`

**Interfaces:**
- Produces (public fields on `StudentGameConfig` ScriptableObject), consumed by `PlayerStats.ApplyConfigIfPresent()` in Task 3:
  - `int startingMoney`, `int startingDebt`
  - `float startingEnergy`, `maxEnergy`, `startingSanity`, `maxSanity`, `startingKnowledge`, `maxKnowledge`
  - `float classStudyEnergyCost`, `classStudySanityCost`, `classStudyKnowledgeGain`
  - `float workEnergyCost`, `workSanityCost`; `int workMoneyGain`
  - `float pcStudyEnergyCost`, `pcStudySanityCost`, `pcStudyKnowledgeGain`
  - `float entertainEnergyCost`, `entertainSanityGain`
  - `float sleepEnergyRestore`, `sleepSanityRestore`
  - `int dailyLivingCost`

- [ ] **Step 1: Replace the whole file**

Replace the contents of `Script/StudentGameConfig.cs` with:

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "StudentGameConfig", menuName = "HUST Student/Game Config")]
public class StudentGameConfig : ScriptableObject
{
    [Header("Starting Stats")]
    public int startingMoney = 5_000_000;
    public int startingDebt = 30_000_000;
    public float startingEnergy = 100f;
    public float maxEnergy = 100f;
    public float startingSanity = 100f;
    public float maxSanity = 100f;
    public float startingKnowledge = 0f;
    public float maxKnowledge = 200f;

    [Header("Classroom Study")]
    public float classStudyEnergyCost = 15f;
    public float classStudySanityCost = 10f;
    public float classStudyKnowledgeGain = 12f;

    [Header("Work (Cafe) — flat pay until mini-game ships")]
    public float workEnergyCost = 20f;
    public float workSanityCost = 8f;
    public int workMoneyGain = 500_000;

    [Header("PC Study (Bedroom)")]
    public float pcStudyEnergyCost = 10f;
    public float pcStudySanityCost = 5f;
    public float pcStudyKnowledgeGain = 6f;

    [Header("Entertain (Bedroom)")]
    public float entertainEnergyCost = 2f;
    public float entertainSanityGain = 15f;

    [Header("Sleep (partial restore)")]
    public float sleepEnergyRestore = 40f;
    public float sleepSanityRestore = 10f;

    [Header("Daily Living Cost")]
    public int dailyLivingCost = 50_000;
}
```

- [ ] **Step 2: Verify compile + update the existing asset**

In Unity Console: zero compile errors. The existing `Assets/ScriptableObjects/DefaultStudentConfig.asset` will keep only the fields whose names still match (`startingMoney`, `startingDebt`, `workMoneyGain`, `dailyLivingCost`); the removed fields drop and the new ones take their defaults. Open the asset in the Inspector once to confirm the new headers appear. (Re-balancing values is done later, not in this task.)

- [ ] **Step 3: Commit**

```bash
git add "Assets/Scenes/class-programing/Assets/_game/Script/StudentGameConfig.cs"
git commit -m "feat: replace config fields with Energy/Sanity/Knowledge stat model"
```

---

### Task 3: PlayerStats rewrite

**Files:**
- Modify (full rewrite): `Script/PlayerStats.cs`

**Interfaces:**
- Consumes: all of `GameRules` (Task 1) and `StudentGameConfig` fields (Task 2); `TimeManager.AdvanceSlot()` and `TimeManager.EndDay()` (existing, confirmed in Task 5).
- Produces (used by EndingManager in Task 4 and ActionTrigger in Task 6):
  - `public float Knowledge { get; }`
  - `public int TotalDebt { get; }`
  - `public float Gpa { get; }` (computed via `GameRules.GpaFor`)
  - Activity methods (all `public void`, wired to scene events): `Study()`, `Work()`, `StudyPC()`, `EntertainPC()`, `Sleep()`, `PayTuition(int amount)`, `OnNewDay()`, `UpdateUI()`

- [ ] **Step 1: Replace the whole file**

Replace the contents of `Script/PlayerStats.cs` with:

```csharp
using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("Config (optional ScriptableObject)")]
    [SerializeField] StudentGameConfig config;

    [Header("Stats")]
    [SerializeField] int money = 5_000_000;
    [SerializeField] int totalDebt = 30_000_000;
    [SerializeField] float energy = 100f;
    [SerializeField] float maxEnergy = 100f;
    [SerializeField] float sanity = 100f;
    [SerializeField] float maxSanity = 100f;
    [SerializeField] float knowledge = 0f;
    [SerializeField] float maxKnowledge = 200f;

    [Header("Classroom Study")]
    [SerializeField] float classStudyEnergyCost = 15f;
    [SerializeField] float classStudySanityCost = 10f;
    [SerializeField] float classStudyKnowledgeGain = 12f;

    [Header("Work (Cafe)")]
    [SerializeField] float workEnergyCost = 20f;
    [SerializeField] float workSanityCost = 8f;
    [SerializeField] int workMoneyGain = 500_000;

    [Header("PC Study (Bedroom)")]
    [SerializeField] float pcStudyEnergyCost = 10f;
    [SerializeField] float pcStudySanityCost = 5f;
    [SerializeField] float pcStudyKnowledgeGain = 6f;

    [Header("Entertain (Bedroom)")]
    [SerializeField] float entertainEnergyCost = 2f;
    [SerializeField] float entertainSanityGain = 15f;

    [Header("Sleep")]
    [SerializeField] float sleepEnergyRestore = 40f;
    [SerializeField] float sleepSanityRestore = 10f;

    [Header("Daily Living Cost")]
    [SerializeField] int dailyLivingCost = 50_000;

    [Header("References")]
    [SerializeField] TimeManager timeManager;

    [Header("UI")]
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI sanityText;
    public TextMeshProUGUI knowledgeText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI debtText;
    public TextMeshProUGUI messageText;

    bool _breakdownPending;
    string _shownEnergy, _shownSanity, _shownKnowledge, _shownMoney, _shownDebt;

    public float Knowledge => knowledge;
    public float Gpa => GameRules.GpaFor(knowledge);
    public int Money => money;
    public float Energy => energy;
    public float Sanity => sanity;
    public int TotalDebt => totalDebt;

    void Awake()
    {
        if (timeManager == null)
            timeManager = FindFirstObjectByType<TimeManager>();
    }

    void Start()
    {
        ApplyConfigIfPresent();
        UpdateUI();
    }

    void ApplyConfigIfPresent()
    {
        if (config == null) return;

        money = config.startingMoney;
        totalDebt = config.startingDebt;
        energy = config.startingEnergy;
        maxEnergy = config.maxEnergy;
        sanity = config.startingSanity;
        maxSanity = config.maxSanity;
        knowledge = config.startingKnowledge;
        maxKnowledge = config.maxKnowledge;

        classStudyEnergyCost = config.classStudyEnergyCost;
        classStudySanityCost = config.classStudySanityCost;
        classStudyKnowledgeGain = config.classStudyKnowledgeGain;

        workEnergyCost = config.workEnergyCost;
        workSanityCost = config.workSanityCost;
        workMoneyGain = config.workMoneyGain;

        pcStudyEnergyCost = config.pcStudyEnergyCost;
        pcStudySanityCost = config.pcStudySanityCost;
        pcStudyKnowledgeGain = config.pcStudyKnowledgeGain;

        entertainEnergyCost = config.entertainEnergyCost;
        entertainSanityGain = config.entertainSanityGain;

        sleepEnergyRestore = config.sleepEnergyRestore;
        sleepSanityRestore = config.sleepSanityRestore;

        dailyLivingCost = config.dailyLivingCost;
    }

    public void Study()
    {
        if (HandleBreakdown()) return;

        float s0 = sanity;
        float energyCost = classStudyEnergyCost * GameRules.StressedEnergyMultiplier(s0);
        if (!HasEnergy(energyCost, "study")) return;

        energy -= energyCost;
        sanity = Mathf.Max(0f, sanity - classStudySanityCost);
        float gain = classStudyKnowledgeGain * GameRules.StressedKnowledgeMultiplier(s0);
        knowledge = Mathf.Min(maxKnowledge, knowledge + gain);

        ShowMessage($"Studied in class. Knowledge +{gain:F0}");
        FinishActivity();
    }

    public void Work()
    {
        if (HandleBreakdown()) return;

        float energyCost = workEnergyCost * GameRules.StressedEnergyMultiplier(sanity);
        if (!HasEnergy(energyCost, "work a shift")) return;

        energy -= energyCost;
        sanity = Mathf.Max(0f, sanity - workSanityCost);
        money += workMoneyGain;

        ShowMessage($"Cafe shift done. +{workMoneyGain:N0} VND");
        FinishActivity();
    }

    public void StudyPC()
    {
        if (HandleBreakdown()) return;

        float s0 = sanity;
        float energyCost = pcStudyEnergyCost * GameRules.StressedEnergyMultiplier(s0);
        if (!HasEnergy(energyCost, "study at the PC")) return;

        energy -= energyCost;
        sanity = Mathf.Max(0f, sanity - pcStudySanityCost);
        float gain = pcStudyKnowledgeGain * GameRules.StressedKnowledgeMultiplier(s0);
        knowledge = Mathf.Min(maxKnowledge, knowledge + gain);

        ShowMessage($"Studied online. Knowledge +{gain:F0}");
        FinishActivity();
    }

    public void EntertainPC()
    {
        if (HandleBreakdown()) return;

        float energyCost = entertainEnergyCost * GameRules.StressedEnergyMultiplier(sanity);
        if (!HasEnergy(energyCost, "relax")) return;

        energy -= energyCost;
        sanity = Mathf.Min(maxSanity, sanity + entertainSanityGain);

        ShowMessage($"Relaxed at the PC. Insanity +{entertainSanityGain:F0}");
        FinishActivity();
    }

    public void Sleep()
    {
        energy = Mathf.Min(maxEnergy, energy + sleepEnergyRestore);
        sanity = Mathf.Min(maxSanity, sanity + sleepSanityRestore);
        _breakdownPending = false;
        ShowMessage("Slept. Energy and sanity partly restored.");
        UpdateUI();
        timeManager?.EndDay();
    }

    public void PayTuition(int amount)
    {
        if (money < amount)
        {
            ShowMessage("Not enough money to pay tuition.");
            return;
        }

        money -= amount;
        totalDebt = Mathf.Max(0, totalDebt - amount);
        ShowMessage($"Paid {amount:N0} VND toward tuition debt.");
        UpdateUI();

        if (totalDebt <= 0)
            ShowMessage("Debt cleared! A better ending is in reach.");
    }

    public void OnNewDay()
    {
        if (dailyLivingCost <= 0) return;

        money -= dailyLivingCost;
        ShowMessage($"Daily expenses: -{dailyLivingCost:N0} VND");
        UpdateUI();
    }

    // When a breakdown is pending, the next chosen action is wasted: the slot is
    // consumed, no stat changes apply, and Sanity recovers to the breakdown floor.
    bool HandleBreakdown()
    {
        if (!_breakdownPending) return false;

        _breakdownPending = false;
        sanity = Mathf.Min(maxSanity, GameRules.BreakdownRecoverTo);
        ShowMessage("You broke down from stress. The slot is lost — but you caught your breath.");
        timeManager?.AdvanceSlot();
        UpdateUI();
        return true;
    }

    void FinishActivity()
    {
        if (sanity <= 0f)
        {
            sanity = 0f;
            _breakdownPending = true;
        }
        timeManager?.AdvanceSlot();
        UpdateUI();
    }

    bool HasEnergy(float cost, string action)
    {
        if (energy >= cost) return true;

        ShowMessage($"Too exhausted to {action}. Need {cost:F0} energy (have {energy:F0}).");
        return false;
    }

    void ShowMessage(string message)
    {
        Debug.Log("[HUST Student] " + message);
        if (messageText != null)
            messageText.text = message;
    }

    public void UpdateUI()
    {
        SetText(energyText, ref _shownEnergy, $"Energy: {energy:F0} / {maxEnergy:F0}");
        SetText(sanityText, ref _shownSanity, $"Insanity: {sanity:F0} / {maxSanity:F0}");
        SetText(knowledgeText, ref _shownKnowledge, $"Knowledge: {knowledge:F0} / {maxKnowledge:F0}");
        SetText(moneyText, ref _shownMoney, $"Money: {money:N0} VND");
        SetText(debtText, ref _shownDebt, $"Tuition Debt: {totalDebt:N0} VND");
    }

    void SetText(TextMeshProUGUI field, ref string cache, string value)
    {
        if (field != null && value != cache)
        {
            field.text = value;
            cache = value;
        }
    }
}
```

- [ ] **Step 2: Verify compile**

Unity Console: zero errors. References to `GameRules` resolve because `HUST.Core` is `autoReferenced`. Old `gpa`/`stamina`/`Rest()` references are gone — if the Console reports a missing `Rest`/`gpaText` from a scene event or binding, that is expected and is fixed by manual rewiring in Task 6's verification.

- [ ] **Step 3: Commit**

```bash
git add "Assets/Scenes/class-programing/Assets/_game/Script/PlayerStats.cs"
git commit -m "feat: rework PlayerStats into Energy/Sanity/Knowledge model with debuff"
```

---

### Task 4: EndingManager

**Files:**
- Create: `Script/EndingManager.cs`

**Interfaces:**
- Consumes: `PlayerStats.Knowledge`, `PlayerStats.TotalDebt` (Task 3); `GameRules.SelectEnding`, `GameRules.GpaFor`, `GameRules.TitleOf` (Task 1).
- Produces: `public void ShowEnding()` — called by `TimeManager.EndDay()` in Task 5.

- [ ] **Step 1: Create the file**

Create `Script/EndingManager.cs`:

```csharp
using UnityEngine;
using TMPro;

public class EndingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerStats playerStats;

    [Header("UI")]
    public GameObject endingPanel;
    public TextMeshProUGUI endingText;

    void Awake()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
    }

    public void ShowEnding()
    {
        float knowledge = playerStats != null ? playerStats.Knowledge : 0f;
        int debt = playerStats != null ? playerStats.TotalDebt : 0;

        var ending = GameRules.SelectEnding(knowledge, debt);
        float gpa = GameRules.GpaFor(knowledge);

        string body =
            $"{GameRules.TitleOf(ending)}\n\n" +
            $"Final GPA: {gpa:F1}\n" +
            $"Debt: {debt:N0} VND";

        Debug.Log("[Ending] " + body);

        if (endingText != null) endingText.text = body;
        if (endingPanel != null) endingPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}
```

- [ ] **Step 2: Verify compile**

Unity Console: zero errors.

- [ ] **Step 3: Commit**

```bash
git add "Assets/Scenes/class-programing/Assets/_game/Script/EndingManager.cs"
git commit -m "feat: add EndingManager to evaluate and display the semester ending"
```

---

### Task 5: TimeManager semester end

**Files:**
- Modify (full rewrite): `Script/TimeManager.cs`

**Interfaces:**
- Consumes: `EndingManager.ShowEnding()` (Task 4); `PlayerStats.OnNewDay()` (existing/Task 3).
- Produces (unchanged from current): `public void AdvanceSlot()`, `public void EndDay()`, `public DaySlot CurrentSlot`. Adds `public int semesterDays`.

- [ ] **Step 1: Replace the whole file**

Replace the contents of `Script/TimeManager.cs` with:

```csharp
using UnityEngine;
using TMPro;

public enum DaySlot { Morning, Afternoon, Evening }

public class TimeManager : MonoBehaviour
{
    [Header("Time")]
    public int day = 1;

    [Header("Semester")]
    public int semesterDays = 10;

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
        // Evening: intentional no-op — player stays until they Sleep
        UpdateUI();
    }

    public void EndDay()
    {
        // Sleeping on the final day ends the semester instead of starting a new day.
        if (day >= semesterDays)
        {
            endingManager?.ShowEnding();
            return;
        }

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

- [ ] **Step 2: Verify compile**

Unity Console: zero errors.

- [ ] **Step 3: Commit**

```bash
git add "Assets/Scenes/class-programing/Assets/_game/Script/TimeManager.cs"
git commit -m "feat: end the semester and trigger the ending on the final day"
```

---

### Task 6: ActionTrigger station types + final wiring

**Files:**
- Modify: `Script/ActionTrigger.cs`

**Interfaces:**
- Consumes: nothing new in code (prompt labels only). The `onInteract` UnityEvent is wired in-scene to `PlayerStats` activity methods.

- [ ] **Step 1: Extend the station enum**

In `Script/ActionTrigger.cs`, replace:

```csharp
    public enum StationType { Study, Work, Rest, Custom }
```

with:

```csharp
    public enum StationType { Study, Work, PCStudy, Entertain, Sleep, Custom }
```

- [ ] **Step 2: Update the prompt labels**

In `ActionTrigger.ShowPrompt`, replace the `switch` expression:

```csharp
            string label = stationType switch
            {
                StationType.Study => "Press E to Study",
                StationType.Work => "Press E to Work at Cafe",
                StationType.Rest => "Press E to Rest",
                _ => promptMessage
            };
```

with:

```csharp
            string label = stationType switch
            {
                StationType.Study => "Press E to Study",
                StationType.Work => "Press E to Work at Cafe",
                StationType.PCStudy => "Press E to Study on PC",
                StationType.Entertain => "Press E to Relax",
                StationType.Sleep => "Press E to Sleep",
                _ => promptMessage
            };
```

- [ ] **Step 3: Verify compile**

Unity Console: zero errors.

- [ ] **Step 4: Commit**

```bash
git add "Assets/Scenes/class-programing/Assets/_game/Script/ActionTrigger.cs"
git commit -m "feat: add PC-study, entertain, and sleep station prompts"
```

- [ ] **Step 5: Manual scene rewiring (Inspector, no commit unless scene is saved)**

The redesign renamed stats and methods, so update the scene:

1. **Player GameObject (`PlayerStats`):** assign the new UI text fields — `energyText`, `sanityText`, `knowledgeText`, `moneyText`, `debtText`, `messageText`. The old `gpaText`/`staminaText` slots no longer exist.
2. **Station triggers (`ActionTrigger`):** set each station's `StationType` and re-point `onInteract` to the matching `PlayerStats` method — classroom → `Study`, cafe → `Work`, bedroom PC study → `StudyPC`, bedroom relax → `EntertainPC`, bed → `Sleep`. The retired `Rest` method will read as a missing function on any event still pointing at it — fix those.
3. **TimeManager GameObject:** set `semesterDays = 10`, and assign `playerStats` and `endingManager` references.
4. **EndingManager:** create a GameObject with `EndingManager`, assign `playerStats`, and wire `endingPanel` (a UI panel, initially inactive) and `endingText`.

---

### Task 7: Play Mode verification

**Files:** None — verification only.

- [ ] **Step 1: Start of game**

Enter Play Mode. UI shows Energy 100, Insanity 100, Knowledge 0, the starting Money and Debt, "Morning", "Day 1".

- [ ] **Step 2: Classroom study**

Trigger `Study`. Energy −15, Insanity −10, Knowledge +12, slot advances to Afternoon.

- [ ] **Step 3: Work**

Trigger `Work`. Energy −20, Insanity −8, Money + workMoneyGain, slot advances.

- [ ] **Step 4: Stressed debuff**

Drive Insanity below 25 (repeated Study/Work across days). Confirm a Study then grants **+6** Knowledge (12 × 0.5) and costs **~19** Energy (15 × 1.25).

- [ ] **Step 5: Breakdown**

Drive Insanity to 0. The next chosen action shows the breakdown message, consumes the slot with no stat gain, and Insanity returns to 15.

- [ ] **Step 6: Energy gate**

Spend Energy below an activity's cost. Confirm that activity refuses with the "Too exhausted" message, while Entertain/Sleep still work.

- [ ] **Step 7: Entertain + Sleep**

Trigger `EntertainPC`: Insanity +15, earns nothing, slot advances. Trigger `Sleep`: Energy and Insanity partially restore and the day advances (Day 2, Morning).

- [ ] **Step 8: Endings (all four cells)**

For each combination, set Knowledge and Debt (via the Inspector on PlayerStats, or by playing) then Sleep on day 10:
  - Knowledge ≥ 180 + Debt 0 → **Honors Graduate**
  - Knowledge ≥ 180 + Debt > 0 → **Brilliant but Broke**
  - Knowledge < 180 + Debt 0 → **Free but Adrift**
  - Knowledge < 180 + Debt > 0 → **Crushed**
Confirm the ending panel shows the title, the final GPA (Knowledge/200×4, 1 decimal), and the debt, and the game freezes.

- [ ] **Step 9: Exit Play Mode**
```
