using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("Config (optional ScriptableObject)")]
    [SerializeField] StudentGameConfig config;

    [Header("Stats")]
    [SerializeField] float gpa = 2.0f;
    [SerializeField] int money = 5000000;
    [SerializeField] float stamina = 100f;
    [SerializeField] float maxStamina = 100f;
    [SerializeField] int totalDebt = 30000000;
    [SerializeField] float maxGpa = 4.0f;

    [Header("Study Parameters")]
    [SerializeField] float studyStaminaCost = 20f;
    [SerializeField] float studyGpaGain = 0.1f;
    [SerializeField] float studyHours = 2f;

    [Header("Work at Cafe Parameters")]
    [SerializeField] float workStaminaCost = 5f;
    [SerializeField] int workMoneyGain = 500000;
    [SerializeField] float workHours = 4f;

    [Header("Rest Parameters")]
    [SerializeField] float restStaminaRestore = 100f;
    [SerializeField] float restHours = 8f;
    [SerializeField] bool restSetsStaminaToMax = true;

    [Header("Daily Living Cost")]
    [SerializeField] int dailyLivingCost = 50000;

    [Header("References")]
    [SerializeField] TimeManager timeManager;

    [Header("UI")]
    public TextMeshProUGUI gpaText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI debtText;
    public TextMeshProUGUI messageText;

    private void Awake()
    {
        if (timeManager == null)
            timeManager = FindFirstObjectByType<TimeManager>();
    }

    private void Start()
    {
        ApplyConfigIfPresent();
        UpdateUI();
    }

    private void ApplyConfigIfPresent()
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
        studyHours = config.studyHours;

        workStaminaCost = config.workStaminaCost;
        workMoneyGain = config.workMoneyGain;
        workHours = config.workHours;

        restStaminaRestore = config.restStaminaRestore;
        restHours = config.restHours;
        restSetsStaminaToMax = config.restSetsStaminaToMax;

        dailyLivingCost = config.dailyLivingCost;
    }

    // ==========================
    // GIAO HÀNG CAFE
    // ==========================
    public bool DeliverCoffee(int earnings)
    {
        if (!HasStamina(workStaminaCost, "giao hàng"))
            return false;

        stamina -= workStaminaCost;
        money += earnings;

        AdvanceTime(workHours);

        ShowMessage(
            $"Giao hàng thành công! +{earnings:N0} VND | Thể lực -{workStaminaCost:F0}"
        );

        UpdateUI();
        return true;
    }

    public void Study()
    {
        if (!HasStamina(studyStaminaCost, "học"))
            return;

        stamina -= studyStaminaCost;
        gpa = Mathf.Min(maxGpa, gpa + studyGpaGain);

        AdvanceTime(studyHours);

        ShowMessage(
            $"Đã học ở thư viện HUST. GPA +{studyGpaGain:F1}"
        );

        UpdateUI();
    }

    public void Work()
    {
        if (!HasStamina(workStaminaCost, "làm ca quán café"))
            return;

        stamina -= workStaminaCost;
        money += workMoneyGain;

        AdvanceTime(workHours);

        ShowMessage(
            $"Ca làm xong. +{workMoneyGain:N0} VND"
        );

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
            stamina = Mathf.Min(
                maxStamina,
                stamina + restStaminaRestore
            );

        AdvanceTime(restHours);

        ShowMessage(
            "Đã nghỉ ngơi ở ký túc xá. Thể lực đã được hồi phục."
        );

        UpdateUI();
    }

    public void PayTuition(int amount)
    {
        if (money < amount)
        {
            ShowMessage("Không đủ tiền đóng học phí.");
            return;
        }

        money -= amount;
        totalDebt = Mathf.Max(0, totalDebt - amount);

        ShowMessage(
            $"Đã đóng {amount:N0} VND học phí."
        );

        UpdateUI();

        if (totalDebt <= 0)
        {
            ShowMessage(
                "Đã trả hết nợ! Mở khóa tốt nghiệp HUST!"
            );
        }
    }

    public void OnNewDay()
    {
        if (dailyLivingCost <= 0)
            return;

        money -= dailyLivingCost;

        ShowMessage(
            $"Chi phí sinh hoạt hàng ngày: -{dailyLivingCost:N0} VND"
        );

        UpdateUI();

        if (money < 0)
        {
            ShowMessage(
                "Cảnh báo: Bạn đang hết tiền! Hãy đi làm hoặc học bổng."
            );
        }
    }

    private bool HasStamina(
        float cost,
        string actionName)
    {
        if (stamina >= cost)
            return true;

        ShowMessage(
            $"Quá mệt để {actionName}. Cần {cost:F0} thể lực (hiện có {stamina:F0}). Hãy nghỉ ngơi!"
        );

        return false;
    }

    private void AdvanceTime(float hours)
    {
        if (timeManager != null)
            timeManager.SkipTime(hours);
    }

    private void ShowMessage(string message)
    {
        Debug.Log("[HUST Student] " + message);

        if (messageText != null)
            messageText.text = message;
    }

    public void UpdateUI()
    {
        if (gpaText != null)
            gpaText.text = $"GPA: {gpa:F1} / {maxGpa:F1}";

        if (moneyText != null)
            moneyText.text = $"Tiền: {money:N0} VND";

        if (staminaText != null)
            staminaText.text =
                $"Thể lực: {stamina:F0} / {maxStamina:F0}";

        if (debtText != null)
            debtText.text =
                $"Học phí còn lại: {totalDebt:N0} VND";
    }

    public float Gpa => gpa;
    public int Money => money;
    public float Stamina => stamina;
    public int TotalDebt => totalDebt;
}