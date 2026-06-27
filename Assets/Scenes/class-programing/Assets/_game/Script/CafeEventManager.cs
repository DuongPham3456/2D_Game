using UnityEngine;

public class CafeEventManager : MonoBehaviour
{
    public TimeManager timeManager;
    
    [Header("NPCs")]
    public GameObject npcTreCon;       // Kéo vật thể NPC_TreCon vào đây
    public GameObject npcKhachBungTien; // Kéo vật thể Khách quỵt tiền vào đây

    void Start()
    {
        // 1. Tắt hết NPC đi để đảm bảo an toàn
        if (npcTreCon != null) npcTreCon.SetActive(false);
        if (npcKhachBungTien != null) npcKhachBungTien.SetActive(false);

        // 2. Tìm kiếm TimeManager nếu bạn quên không kéo vào
        if (timeManager == null)
            timeManager = FindFirstObjectByType<TimeManager>();

        // 3. Kiểm tra ngày để thả NPC ra
        if (timeManager.day == 3)
        {
            if (npcTreCon != null) npcTreCon.SetActive(true);
        }
        else if (timeManager.day == 1)
        {
            if (npcKhachBungTien != null) npcKhachBungTien.SetActive(true);
        }
    }
}