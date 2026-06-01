using UnityEngine;
using UnityEngine.Events;

public class ActionTrigger : MonoBehaviour
{
    [Header("Kéo thả hàm vào đây")]
    public UnityEvent onInteract;

    private bool isPlayerInRange = false;

    void Start()
    {
        // Kiểm tra xem script có đang chạy không
        Debug.Log("🚀 Trạm [" + gameObject.name + "] đã khởi động thành công!");
    }

    void Update()
    {
        // MỖI KHI BẠN BẤM E, DÒNG NÀY PHẢI HIỆN RA (Dù đứng ở đâu)
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("⌨️ Phím E vừa được bấm!");
            
            if (isPlayerInRange)
            {
                Debug.Log("✅ Đang đứng đúng vị trí. Kích hoạt sự kiện ngay!");
                onInteract.Invoke();
            }
            else
            {
                Debug.Log("❌ Bạn bấm E, nhưng Unity nghĩ bạn CHƯA ĐỨNG VÀO TRONG TRẠM.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // MỖI KHI CÓ VẬT CHẠM VÀO TRẠM (kể cả không phải Player), DÒNG NÀY SẼ HIỆN
        Debug.Log("💥 Va chạm! Có một vật tên là [" + other.gameObject.name + "] vừa bước vào. Tag của nó là: " + other.gameObject.tag);

        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("🎯 Chuẩn Player rồi. Sẵn sàng bấm E.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("🚶 Player đã đi ra ngoài.");
        }
    }
}