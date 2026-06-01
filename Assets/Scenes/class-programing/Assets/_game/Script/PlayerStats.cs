using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("Chỉ số sinh viên")]
    public float gpa = 0f;
    public int money = 0;
    public float stamina = 100f;
    public float maxStamina = 100f;
    public int totalDebt = 150000000;

    [Header("UI Elements (Nhớ kéo UI vào đây)")]
    public TextMeshProUGUI gpaText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI debtText;

    void Start()
    {
        UpdateUI();
    }

    public void Study()
    {
        Debug.Log("Đã gọi hàm Study!"); 
        if (stamina >= 20f)
        {
            gpa = Mathf.Min(4.0f, gpa + 0.1f);
            stamina -= 20f;
            UpdateUI();
            Debug.Log("Học thành công: +0.1 GPA, -20 Thể lực");
        }
        else
        {
            Debug.Log("Burnout! Không đủ 20 thể lực để học.");
        }
    }

    public void Work()
    {
        Debug.Log("Đã gọi hàm Work!");
        if (stamina >= 30f)
        {
            money += 500000;
            stamina -= 30f;
            UpdateUI();
            Debug.Log("Làm việc thành công: +500k, -30 Thể lực");
        }
        else
        {
            Debug.Log("Burnout! Không đủ 30 thể lực để đi làm.");
        }
    }

    public void Sleep()
    {
        Debug.Log("Đã gọi hàm Sleep!");
        stamina = maxStamina;
        UpdateUI();
        Debug.Log("Ngủ thành công: Hồi 100% Thể lực");
    }

    // ĐÂY LÀ HÀM ĐÓNG HỌC PHÍ VỪA ĐƯỢC THÊM VÀO
    public void PayTuition(int amount)
    {
        Debug.Log("Đã gọi hàm PayTuition!");
        if (money >= amount)
        {
            money -= amount;
            totalDebt -= amount;
            UpdateUI();
            Debug.Log("Đóng tiền thành công: Trừ " + amount + " VNĐ tiền nợ!");
            
            if (totalDebt <= 0)
            {
                Debug.Log("🎉 CHIẾN THẮNG! Bạn đã trả hết nợ và tốt nghiệp!");
            }
        }
        else
        {
            Debug.Log("Thất bại! Bạn không đủ tiền đóng khoản này.");
        }
    }

    public void UpdateUI()
    {
        if (gpaText != null) gpaText.text = "GPA: " + gpa.ToString("F1");
        if (moneyText != null) moneyText.text = "Tiền: " + money.ToString("N0") + " VNĐ";
        if (staminaText != null) staminaText.text = "Thể lực: " + stamina.ToString("F0") + "/" + maxStamina;
        if (debtText != null) debtText.text = "Nợ: " + totalDebt.ToString("N0") + " VNĐ";
    }
}