using UnityEngine;
using TMPro;

public class NotificationManager : MonoBehaviour
{
    // Tạo biến Instance để mọi file khác đều có thể gọi đến script này mà không cần FindObject
    public static NotificationManager Instance;

    public GameObject UI_Chu;        // Kéo vật thể UI_ThongBaoSuKien vào đây
    public TextMeshProUGUI textMesh; // Kéo chính vật thể đó vào đây nốt

    void Awake()
    {
        Instance = this;
    }

    // Hàm nhận nội dung và hiện chữ lên
    public void HienThongBao(string noiDung, float thoiGianHien = 3f)
    {
        if (UI_Chu != null && textMesh != null)
        {
            textMesh.text = noiDung;
            UI_Chu.SetActive(true);
            
            // Xóa bộ đếm cũ và đặt giờ tự động tắt chữ
            CancelInvoke("TatThongBao");
            Invoke("TatThongBao", thoiGianHien);
        }
    }

    void TatThongBao()
    {
        if (UI_Chu != null) UI_Chu.SetActive(false);
    }
}