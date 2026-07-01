using UnityEngine;

// Customer who skips the bill. Press F near them -> they flee and you lose energy.
public class NPC_KhachQuytTien : MonoBehaviour
{
    public float runSpeed = 6f;
    public KeyCode interactKey = KeyCode.F;
    public Animator anim;
    [SerializeField] float energyPenalty = 10f;

    bool isPlayerNear;
    bool isRunningAway;

    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!UIModal.IsOpen && isPlayerNear && !isRunningAway && Input.GetKeyDown(interactKey))
        {
            NotificationManager.Instance?.Show("The customer dined and dashed!", 4f);
            FindFirstObjectByType<PlayerStats>()?.ApplyDailyEvent(0, -energyPenalty, "Dine-and-dash at the cafe");

            isRunningAway = true;
            if (anim != null) anim.SetBool("isRunning", true);
        }

        if (isRunningAway)
            transform.Translate(Vector2.right * runSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNear = true;
        if (isRunningAway && other.CompareTag("Wall")) gameObject.SetActive(false);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNear = false;
    }
}
