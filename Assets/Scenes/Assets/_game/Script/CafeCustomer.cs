using UnityEngine;

// A cafe customer: walks in from the door to a waiting spot, waits for coffee, and
// once served (bring a coffee, press E) pays out and walks back to the door to leave.
// Same reward/loop as the Delivery Counter, just on a spawned, moving NPC.
public class CafeCustomer : MonoBehaviour
{
    enum Phase { WalkingIn, Waiting, Leaving }

    [SerializeField] int reward = 15000;
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] Animator anim;   // Idle while waiting, isWalking=true while moving

    CafeShiftManager shift;
    PlayerStats stats;
    PlayerManager player;
    bool isPlayerNearby;

    Phase phase = Phase.Waiting;   // default: stand where placed unless told to walk in
    Vector3 waitTarget;
    Vector3 exitTarget;            // the door — where they spawned and where they return to leave

    void Awake()
    {
        // These live on the game manager, not the player — find them directly.
        shift = FindFirstObjectByType<CafeShiftManager>();
        stats = FindFirstObjectByType<PlayerStats>();
        if (anim == null) anim = GetComponent<Animator>();
        exitTarget = transform.position;   // spawn spot = the door they leave through
    }

    // Spawner calls this right after Instantiate so the customer strolls in.
    public void WalkInTo(Vector3 target)
    {
        waitTarget = target;
        phase = Phase.WalkingIn;
        SetWalking(true);
    }

    void Update()
    {
        switch (phase)
        {
            case Phase.WalkingIn: MoveTowardWait(); break;
            case Phase.Waiting:   HandleServeInput(); break;
            case Phase.Leaving:   MoveTowardExit(); break;
        }
    }

    void MoveTowardWait()
    {
        if (WalkToward(waitTarget))
        {
            phase = Phase.Waiting;
            SetWalking(false);   // arrived — stand and wait
        }
    }

    void MoveTowardExit()
    {
        if (WalkToward(exitTarget))
            Destroy(gameObject);   // reached the door — gone
    }

    // Walk horizontally toward target.x (keep current Y). Returns true on arrival.
    bool WalkToward(Vector3 target)
    {
        Vector3 p = transform.position;
        Vector3 goal = new Vector3(target.x, p.y, p.z);
        transform.position = Vector3.MoveTowards(p, goal, moveSpeed * Time.deltaTime);
        return Mathf.Abs(transform.position.x - target.x) < 0.05f;
    }

    void HandleServeInput()
    {
        if (UIModal.IsOpen) return;
        if (isPlayerNearby && player != null && Input.GetKeyDown(KeyCode.E))
            TryServe();
    }

    void TryServe()
    {
        if (shift != null && !shift.Active)
        {
            Debug.Log("[Cafe] Clock in for a shift before serving customers.");
            return;
        }
        if (player.myHand != PlayerManager.HandState.HasCoffeeCup)
        {
            Debug.Log("[Cafe] This customer wants a coffee — brew one and bring it over.");
            return;
        }

        // DeliverCoffee pays the reward (minus stress-scaled energy) and can fail
        // if too exhausted — only empty the hand / send them off on success.
        if (stats != null && stats.DeliverCoffee(reward))
        {
            player.myHand = PlayerManager.HandState.Empty;
            phase = Phase.Leaving;
            SetWalking(true);   // walk-away anim
            Debug.Log("[Cafe] Coffee handed to the customer! They leave happy.");
        }
    }

    void SetWalking(bool walking)
    {
        if (anim != null) anim.SetBool("isWalking", walking);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerNearby = true;
        player = other.GetComponent<PlayerManager>();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNearby = false;
        player = null;
    }
}
