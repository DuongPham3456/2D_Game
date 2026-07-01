using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("Config (optional ScriptableObject)")]
    [SerializeField] StudentGameConfig config;

    [Header("Stats")]
    [SerializeField] int money = 200_000;
    [SerializeField] int totalDebt = 3_000_000;
    [SerializeField] float energy = 100f;
    [SerializeField] float maxEnergy = 100f;
    [SerializeField] float sanity = 100f;
    [SerializeField] float maxSanity = 100f;
    [SerializeField] float knowledge = 0f;
    [SerializeField] float maxKnowledge = 200f;

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
    [SerializeField] int dailyLivingCost = 100_000;

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

        pcStudyEnergyCost = config.pcStudyEnergyCost;
        pcStudySanityCost = config.pcStudySanityCost;
        pcStudyKnowledgeGain = config.pcStudyKnowledgeGain;

        entertainEnergyCost = config.entertainEnergyCost;
        entertainSanityGain = config.entertainSanityGain;

        sleepEnergyRestore = config.sleepEnergyRestore;
        sleepSanityRestore = config.sleepSanityRestore;

        dailyLivingCost = config.dailyLivingCost;
    }

    [Header("Cafe Coffee Minigame")]
    [SerializeField] float deliverEnergyCost = 1f;

    // One coffee delivered during a shift: stress-scaled energy cost + money.
    // No slot here — the shift as a whole spends the slot (see CafeShiftManager).
    public bool DeliverCoffee(int reward)
    {
        float cost = deliverEnergyCost * GameRules.StressedEnergyMultiplier(sanity);
        if (!HasEnergy(cost, "serve coffee")) return false;

        energy -= cost;
        money += reward;
        ShowMessage($"Coffee delivered! +{reward:N0} VND");
        UpdateUI();
        return true;
    }

    // Buy food / an energy drink: money -> energy, capped at max. No time slot.
    // Returns false if already full or too broke.
    public bool BuyEnergy(int cost, float energyGain, string itemName)
    {
        if (energy >= maxEnergy)
        {
            ShowMessage("Năng lượng đã đầy.");
            return false;
        }
        if (money < cost)
        {
            ShowMessage($"Không đủ tiền mua {itemName} ({cost:N0} VND).");
            return false;
        }

        money -= cost;
        energy = Mathf.Min(maxEnergy, energy + energyGain);
        ShowMessage($"{itemName}: +{energyGain:F0} năng lượng, -{cost:N0} VND");
        UpdateUI();
        return true;
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

    // Daytime nap at the bed: fully recharge energy and spend one slot — the day
    // continues (unlike Sleep, which ends it). Matches the old bed nap behavior.
    public void Nap()
    {
        energy = maxEnergy;
        ShowMessage("Ngủ một giấc nạp đầy năng lượng!");
        UpdateUI();
        timeManager?.AdvanceSlot();
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

    // Generic scripted/random event: adjust money + energy and show a message.
    // Bypasses sanity/knowledge and does NOT spend a slot — for daily events & NPCs.
    public void ApplyDailyEvent(int moneyChange, float energyChange, string eventMessage)
    {
        money += moneyChange;
        energy = Mathf.Clamp(energy + energyChange, 0f, maxEnergy);
        ShowMessage($"[Event] {eventMessage}");
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

    // --- Desk quiz session: same rules as Study() — breakdown, stress-scaled
    // energy + sanity cost, stress-scaled knowledge reward. ---

    float _quizKnowledgeMultiplier = 1f;

    // Start a quiz session. Returns false if it can't begin (breakdown consumed
    // the slot, or not enough energy) — the desk should not open the book then.
    public bool TryStartQuizStudy(float baseEnergyCost, float sanityCost)
    {
        if (HandleBreakdown()) return false;

        float s0 = sanity;
        float energyCost = baseEnergyCost * GameRules.StressedEnergyMultiplier(s0);
        if (!HasEnergy(energyCost, "study")) return false;

        energy -= energyCost;
        sanity = Mathf.Max(0f, sanity - sanityCost);
        _quizKnowledgeMultiplier = GameRules.StressedKnowledgeMultiplier(s0);
        UpdateUI();
        return true;
    }

    // Reward one correct answer, stress-scaled like classroom study.
    public void AddQuizKnowledge(float baseGain)
    {
        float gain = baseGain * _quizKnowledgeMultiplier;
        knowledge = Mathf.Min(maxKnowledge, knowledge + gain);
        ShowMessage($"Trả lời đúng! Kiến thức +{gain:F0}");
        UpdateUI();
    }

    // End a quiz session — arm a breakdown if sanity bottomed out, like FinishActivity().
    public void EndQuizStudy()
    {
        if (sanity <= 0f)
        {
            sanity = 0f;
            _breakdownPending = true;
        }
        UpdateUI();
    }
}
