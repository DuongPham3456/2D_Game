using UnityEngine;
using TMPro;

public class PC_Manager : MonoBehaviour
{
    [Header("Các Màn Hình")]
    public GameObject panelMenu;
    public GameObject panelText;

    [Header("Giao diện Chữ")]
    public TextMeshProUGUI noiDungText; // Chỗ để hiện chữ đọc

    void OnEnable()
    {
        ShowMenu(); // Cứ bật PC lên là về màn hình chính
    }

    public void ShowMenu()
    {
        panelMenu.SetActive(true);
        panelText.SetActive(false);
    }

    // Nút "HỌC"
    public void MoApp_Hoc()
    {
        panelMenu.SetActive(false);
        panelText.SetActive(true);
        noiDungText.text = "PHẦN MỀM HỌC TẬP\n\nBạn đang cày cuốc tài liệu... Kiến thức đang chảy vào đầu!";
    }

    // Nút "CUỘC ĐỜI"
    public void MoApp_CuocDoi()
    {
        panelMenu.SetActive(false);
        panelText.SetActive(true);
        noiDungText.text = "CẨM NANG SINH VIÊN:\n\n1. Đừng nợ môn trên trường.\n2. Cố gắng làm tốt file báo cáo thực tập.\n3. Ngủ đủ giấc để không bị suy nhược.";
    }

    // Nút "GIẢI TRÍ" - Đã sửa theo ý bạn
    public void MoApp_GiaiTri()
    {
        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        
        // Kiểm tra xem có đủ 20 thể lực không
        if (stats != null && stats.Energy >= 20)
        {
            // Trừ 20 thể lực dùng cổng ApplyDailyEvent có sẵn của bạn
            stats.ApplyDailyEvent(0, -20f, "Chơi game giải trí");

            // Bật màn hình chữ lên và in câu mắng
            panelMenu.SetActive(false);
            panelText.SetActive(true);
            noiDungText.text = "TRÒ CHƠI GIẢI TRÍ:\n\nBạn quá ham chơi và bị trừ 20đ thể lực trong hôm nay!";
        }
        else
        {
            // Báo lỗi nếu đã quá mệt mà còn cố chơi
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.HienThongBao("Bạn quá mệt! Cần 20 thể lực để chơi game.", 3f);
            }
        }
    }

    // Nút "TẮT MÁY"
    public void TatMayTinh()
    {
        gameObject.SetActive(false); 
    }
}