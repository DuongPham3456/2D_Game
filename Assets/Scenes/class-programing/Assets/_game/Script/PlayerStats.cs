using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("Config (optional ScriptableObject)")]
    [SerializeField] StudentGameConfig config;

    [Header("Stats — modify in Play Mode or Inspector")]
    [SerializeField] float gpa = 2.0f;
    [SerializeField] int money = 5_000_000;
    [SerializeField] float stamina = 100f;
    [SerializeField] float maxStamina = 100f;
    [SerializeField] int totalDebt = 30_000_000;
    [SerializeField] float maxGpa = 4.0f;

    [Header("Study Parameters")]
    [SerializeField] float studyStaminaCost = 20f;
    [SerializeField] float studyGpaGain = 0.1f;

    [Header("Work at Cafe Parameters")]
    [SerializeField] float workStaminaCost = 30f;
    [SerializeField] int workMoneyGain = 500_000;

    [Header("Rest Parameters")]
    [SerializeField] float restStaminaRestore = 100f;
    [SerializeField] bool restSetsStaminaToMax = true;

    [Header("Daily Living Cost")]
    [SerializeField] int dailyLivingCost = 50_000;

    [Header("References")]
    [SerializeField] TimeManager timeManager;

    [Header("UI")]
    public TextMeshProUGUI gpaText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI debtText;
    public TextMeshProUGUI messageText;

    string _shownGpa, _shownMoney, _shownStamina, _shownDebt;

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

        gpa = config.startingGpa;
        money = config.startingMoney;
        stamina = config.startingStamina;
        maxStamina = config.maxStamina;
        totalDebt = config.startingDebt;
        maxGpa = config.maxGpa;

        studyStaminaCost = config.studyStaminaCost;
        studyGpaGain = config.studyGpaGain;

        workStaminaCost = config.workStaminaCost;
        workMoneyGain = config.workMoneyGain;

        restStaminaRestore = config.restStaminaRestore;
        restSetsStaminaToMax = config.restSetsStaminaToMax;

        dailyLivingCost = config.dailyLivingCost;
    }

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

    public void Sleep()
    {
        Rest();
    }

    public void Rest()
    {
        if (restSetsStaminaToMax)
            stamina = maxStamina;
        else
            stamina = Mathf.Min(maxStamina, stamina + restStaminaRestore);

        ShowMessage("Rested at the dorm. Stamina restored.");

        if (timeManager != null && timeManager.CurrentSlot == DaySlot.Evening)
            timeManager.EndDay();
        else
            timeManager?.AdvanceSlot();

        UpdateUI();
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
            ShowMessage("You cleared your debt! HUST graduation unlocked!");
    }

    public void OnNewDay()
    {
        if (dailyLivingCost <= 0) return;

        money -= dailyLivingCost;
        ShowMessage($"Daily expenses: -{dailyLivingCost:N0} VND");
        UpdateUI();

        if (money < 0)
            ShowMessage("Warning: You are broke! Work at the cafe or study for scholarships.");
    }

    bool HasStamina(float cost, string actionName)
    {
        if (stamina >= cost) return true;

        ShowMessage($"Too tired to {actionName}. Need {cost:F0} stamina (have {stamina:F0}). Rest first!");
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
        SetText(gpaText, ref _shownGpa, $"GPA: {gpa:F1} / {maxGpa:F1}");
        SetText(moneyText, ref _shownMoney, $"Money: {money:N0} VND");
        SetText(staminaText, ref _shownStamina, $"Stamina: {stamina:F0} / {maxStamina:F0}");
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

    public float Gpa => gpa;
    public int Money => money;
    public float Stamina => stamina;
    public int TotalDebt => totalDebt;
}
