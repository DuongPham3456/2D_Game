using System.Collections;
using UnityEngine;

// Sleep sequence. Interaction (proximity + E + prompt) is handled by the ActionTrigger
// on this same object; we just react to its Interacted event.
[RequireComponent(typeof(ActionTrigger))]
public class BedInteract : MonoBehaviour
{
    [Header("Tham chiếu")]
    public Animator playerAnimator;
    public Transform diemNamNgu;

    [Header("Âm thanh")]
    public AudioSource sleepSound;

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
    }

    void Sleep()
    {
        if (!isSleeping) StartCoroutine(DiNguCoroutine());
    }

    IEnumerator DiNguCoroutine()
    {
        isSleeping = true;

        // 1. Hút nhân vật về giường & khóa vận tốc
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            if (diemNamNgu != null) playerObject.transform.position = diemNamNgu.position;
            Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        // 2. Animation, đánh tráo giường, phát âm thanh
        if (playerAnimator != null) playerAnimator.SetBool("isSleeping", true);
        if (hinhAnhGiuong != null) hinhAnhGiuong.enabled = false;
        if (sleepSound != null) sleepSound.Play();

        // 3. Chờ 3 giây rồi tỉnh dậy
        yield return new WaitForSeconds(3f);

        if (sleepSound != null && sleepSound.isPlaying) sleepSound.Stop();
        if (hinhAnhGiuong != null) hinhAnhGiuong.enabled = true;
        if (playerAnimator != null) playerAnimator.SetBool("isSleeping", false);

        // 4. Logic thời gian & thể lực
        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        TimeManager time = FindFirstObjectByType<TimeManager>();
        if (stats != null && time != null)
        {
            if (time.CurrentSlot == DaySlot.Evening)
                stats.Sleep();
            else
            {
                stats.ApplyDailyEvent(0, 9999f, "Ngủ một giấc nạp đầy năng lượng!");
                time.AdvanceSlot();
            }
        }

        isSleeping = false;
    }
}
