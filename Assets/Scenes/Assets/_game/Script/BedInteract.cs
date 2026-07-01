using System.Collections;
using UnityEngine;

// Sleep sequence. Interaction (proximity + E + prompt) is handled by the ActionTrigger
// on this same object; we just react to its Interacted event.
[RequireComponent(typeof(ActionTrigger))]
public class BedInteract : MonoBehaviour
{
    [Header("Tham chiếu")]
    public Animator playerAnimator;
    [Tooltip("Để trống = ngủ ngay tại vị trí giường.")]
    public Transform diemNamNgu;

    [Header("Âm thanh")]
    [Tooltip("Kéo thẳng file âm thanh vào đây.")]
    public AudioClip sleepClip;

    private AudioSource sleepSource;
    private SpriteRenderer hinhAnhGiuong;
    private bool isSleeping = false;

    void Awake()
    {
        var trigger = GetComponent<ActionTrigger>();
        if (trigger != null) trigger.Interacted += Sleep;
    }

    void Start()
    {
        hinhAnhGiuong = GetComponent<SpriteRenderer>();

        // Tự tạo AudioSource để chỉ cần gán AudioClip trong Inspector.
        sleepSource = GetComponent<AudioSource>();
        if (sleepSource == null) sleepSource = gameObject.AddComponent<AudioSource>();
        sleepSource.playOnAwake = false;
    }

    void Sleep()
    {
        if (!isSleeping) StartCoroutine(DiNguCoroutine());
    }

    IEnumerator DiNguCoroutine()
    {
        isSleeping = true;
        UIModal.Open();   // ẩn prompt "Press E" & chặn tương tác khác khi đang ngủ

        // 1. Hút nhân vật về giường, khóa vận tốc & khóa di chuyển
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        PlayerLocomotionManager locomotion = null;
        Rigidbody2D rb = null;
        if (playerObject != null)
        {
            rb = playerObject.GetComponent<Rigidbody2D>();
            // Tắt vật lý trước khi dịch chuyển: nếu không, va chạm với collider của
            // giường sẽ đẩy nhân vật văng ra (bay khỏi map). Bật lại khi tỉnh dậy.
            if (rb != null) { rb.linearVelocity = Vector2.zero; rb.simulated = false; }
            // Ngủ tại vị trí giường (hoặc điểm nằm ngủ nếu có gán).
            playerObject.transform.position = diemNamNgu != null ? diemNamNgu.position : transform.position;
            // Không cho đi lại trong lúc ngủ (dùng lại cờ của hệ thống dodge).
            locomotion = playerObject.GetComponent<PlayerLocomotionManager>();
            if (locomotion != null) locomotion.allowToMove = false;
        }

        // 2. Animation, đánh tráo giường, phát âm thanh
        if (playerAnimator != null) playerAnimator.SetBool("isSleeping", true);
        if (hinhAnhGiuong != null) hinhAnhGiuong.enabled = false;
        if (sleepClip != null) { sleepSource.clip = sleepClip; sleepSource.Play(); }

        // 3. Chờ 3 giây rồi tỉnh dậy
        yield return new WaitForSeconds(3f);

        if (sleepSource.isPlaying) sleepSource.Stop();
        if (hinhAnhGiuong != null) hinhAnhGiuong.enabled = true;
        if (playerAnimator != null) playerAnimator.SetBool("isSleeping", false);

        // 4. Logic thời gian & thể lực
        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        TimeManager time = FindFirstObjectByType<TimeManager>();
        if (stats != null && time != null)
        {
            // Evening = full sleep (ends the day); daytime = nap (advances one slot).
            if (time.CurrentSlot == DaySlot.Evening)
                stats.Sleep();
            else
                stats.Nap();
        }

        // Bật lại vật lý & mở khóa di chuyển sau khi tỉnh dậy.
        if (rb != null) rb.simulated = true;
        if (locomotion != null) locomotion.allowToMove = true;

        UIModal.Close();
        isSleeping = false;
    }
}
