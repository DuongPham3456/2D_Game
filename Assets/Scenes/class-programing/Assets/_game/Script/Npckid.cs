using UnityEngine;

public class Npckid : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        // Cho NPC liên tục chạy về bên trái
        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Nếu người chơi KHÔNG NÉ ĐƯỢC và bị đụng trúng
        if (other.CompareTag("Player"))
        {
            Debug.Log("Oạch! Đứa trẻ đâm trúng bạn. Đổ cafe!");

            // 1. HIỆN CHỮ THÔNG BÁO LÊN MÀN HÌNH (Dùng hệ thống Notification)
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.HienThongBao("Oạch! Bị trẻ con đâm trúng. Mất 200k tiền đền rơi đồ!", 3f);
            }

            // 2. TÌM VÀ GỌI HÀM TRỪ TIỀN BÊN PLAYERSTATS (Trừ 200k, không trừ thể lực)
            PlayerStats stats = FindFirstObjectByType<PlayerStats>();
            if (stats != null)
            {
                // Dùng luôn hàm ApplyDailyEvent bạn đã có sẵn ở bài trước để trừ tiền cực kỳ an toàn
                stats.ApplyDailyEvent(-200000, 0f, "Đền tiền cafe do trẻ con làm đổ");
            }
            
            gameObject.SetActive(false); // Đứa trẻ chạy biến mất sau khi đụng trúng
        }
        // Nếu đâm vào bức tường ở cuối bản đồ (nghĩa là người chơi đã né thành công)
        else if (other.CompareTag("Wall")) 
        {
            Debug.Log("Trẻ con đã chạy mất, bạn né thành công!");
            gameObject.SetActive(false); 
        }
    }
}