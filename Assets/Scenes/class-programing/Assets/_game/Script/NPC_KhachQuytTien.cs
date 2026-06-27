using UnityEngine;

public class NPC_KhachQuytTien : MonoBehaviour
{
    public float runSpeed = 6f;       
    public KeyCode interactKey = KeyCode.F; 

    // Thêm biến chứa Animator
    public Animator anim;

    private bool isPlayerNear = false;
    private bool isRunningAway = false;

    void Start()
    {
        // Tự động tìm bộ Animator trên người NPC lúc mới vào game
        if (anim == null) anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(interactKey) && !isRunningAway)
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.HienThongBao("Haha ngu ngốc! Khách bùng tiền chạy mất dép!", 4f);
            }

            PlayerStats stats = FindFirstObjectByType<PlayerStats>();
            if (stats != null)
            {
                stats.ApplyDailyEvent(0, -10f, "Bị quỵt tiền cafe");
            }

            isRunningAway = true;

            // BẬT CÔNG TẮC ANIMATION: Báo cho Unity biết là NPC bắt đầu chạy!
            if (anim != null)
            {
                anim.SetBool("isRunning", true);
            }
        }

        if (isRunningAway)
        {
            transform.Translate(Vector2.right * runSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
        
        if (isRunningAway && other.CompareTag("Wall")) 
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}