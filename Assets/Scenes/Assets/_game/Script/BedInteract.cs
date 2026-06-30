using System.Collections;
using UnityEngine;

public class BedInteract : MonoBehaviour
{
    [Header("Giao diện & Tham chiếu")]
    public GameObject chuPhimE;       
    public Animator playerAnimator;   
    public Transform diemNamNgu; 
    
    [Header("Âm thanh")]
    public AudioSource sleepSound; // Thêm loa phát tiếng ngủ

    private SpriteRenderer hinhAnhGiuong; 
    private bool isPlayerNear = false;
    private bool isSleeping = false;
    private GameObject playerObject; 

    void Start()
    {
        if (chuPhimE != null) chuPhimE.SetActive(false);
        hinhAnhGiuong = GetComponent<SpriteRenderer>(); 
    }

    void Update()
    {
        if (isPlayerNear && !isSleeping && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(DiNguCoroutine());
        }
    }

    IEnumerator DiNguCoroutine()
    {
        isSleeping = true;
        
        // 1. Hút nhân vật & Khóa di chuyển
        if (playerObject != null)
        {
            if (diemNamNgu != null) playerObject.transform.position = diemNamNgu.position;
            Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero; 
        }

        // 2. Bật UI, Animation, đánh tráo giường
        if (chuPhimE != null) chuPhimE.SetActive(false);
        if (playerAnimator != null) playerAnimator.SetBool("isSleeping", true);
        if (hinhAnhGiuong != null) hinhAnhGiuong.enabled = false;

        // 3. PHÁT ÂM THANH ĐI NGỦ
        if (sleepSound != null)
        {
            sleepSound.Play();
        }

        // 4. Chờ 3 giây
        yield return new WaitForSeconds(3f);

        // 5. Tỉnh dậy: Tắt âm thanh (nếu nó dài hơn 3s), hoàn tác hình ảnh
        if (sleepSound != null && sleepSound.isPlaying)
        {
            sleepSound.Stop(); // Tắt nhạc khi đã ngủ dậy
        }
        
        if (hinhAnhGiuong != null) hinhAnhGiuong.enabled = true;
        if (playerAnimator != null) playerAnimator.SetBool("isSleeping", false);
        
        // 6. Xử lý Logic thời gian & thể lực
        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        TimeManager time = FindFirstObjectByType<TimeManager>();

        if (stats != null && time != null)
        {
            if (time.CurrentSlot == DaySlot.Evening)
            {
                stats.Sleep(); 
            }
            else
            {
                stats.ApplyDailyEvent(0, 9999f, "Ngủ một giấc nạp đầy năng lượng!");
                time.AdvanceSlot();
            }
        }

        isSleeping = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerObject = other.gameObject; 
            if (!isSleeping && chuPhimE != null) chuPhimE.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerObject = null; 
            if (chuPhimE != null) chuPhimE.SetActive(false);
        }
    }
}