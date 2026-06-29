using UnityEngine;
using TMPro;

// Bedroom PC menu. The Study and Relax buttons call the real PlayerStats actions
// (knowledge / sanity, and each spends a time slot). The Life guide is flavor only.
public class PC_Manager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject panelMenu;
    public GameObject panelText;

    [Header("Text")]
    public TextMeshProUGUI noiDungText;

    PlayerStats playerStats;

    void Awake()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
    }

    void OnEnable()
    {
        ShowMenu();
    }

    public void ShowMenu()
    {
        panelMenu.SetActive(true);
        panelText.SetActive(false);
    }

    // "Học" — real PC study: gives knowledge, costs energy/sanity, spends a slot.
    public void MoApp_Hoc()
    {
        if (playerStats != null) playerStats.StudyPC();
        ShowText("PHẦN MỀM HỌC TẬP\n\nBạn cày tài liệu... kiến thức đang tăng lên!");
    }

    // "Cuộc đời" — flavor only, no stat change.
    public void MoApp_CuocDoi()
    {
        ShowText("CẨM NANG SINH VIÊN:\n\n1. Đừng nợ môn.\n2. Làm tốt báo cáo thực tập.\n3. Ngủ đủ giấc để không suy nhược.");
    }

    // "Giải trí" — real relax: restores sanity, costs a little energy, spends a slot.
    public void MoApp_GiaiTri()
    {
        if (playerStats != null) playerStats.EntertainPC();
        ShowText("GIẢI TRÍ:\n\nBạn thư giãn một chút, tinh thần thoải mái hơn!");
    }

    public void TatMayTinh()
    {
        gameObject.SetActive(false);
    }

    void ShowText(string content)
    {
        panelMenu.SetActive(false);
        panelText.SetActive(true);
        if (noiDungText != null) noiDungText.text = content;
    }
}
