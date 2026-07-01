using UnityEngine;

// Drips cafe customers in while a shift is Active. Each spawned customer waits to
// be served (bring a coffee, press E). Caps how many wait at once so it can't flood.
public class CafeCustomerSpawner : MonoBehaviour
{
    [SerializeField] GameObject customerPrefab;
    [SerializeField] Transform spawnPoint;      // where they appear (defaults to this object)
    [SerializeField] Transform waitPoint;       // where they walk to and wait (defaults to spawn)
    [SerializeField] float spawnInterval = 20f;
    [SerializeField] int maxWaiting = 1;        // 1 = one at a time, no overlap at the spot

    CafeShiftManager shift;
    float timer;

    void Awake()
    {
        shift = FindFirstObjectByType<CafeShiftManager>();
        if (spawnPoint == null) spawnPoint = transform;
    }

    void Update()
    {
        // Only during a shift; reset the timer so the first customer waits a bit
        // after clock-in rather than popping instantly.
        if (shift == null || !shift.Active) { timer = spawnInterval; return; }

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = spawnInterval;

        if (customerPrefab == null) return;
        if (FindObjectsByType<CafeCustomer>(FindObjectsSortMode.None).Length >= maxWaiting) return;

        var obj = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        var customer = obj.GetComponent<CafeCustomer>();
        if (customer != null) customer.WalkInTo((waitPoint != null ? waitPoint : spawnPoint).position);
        Debug.Log("[Cafe] A customer walked in — bring them a coffee and press E.");
    }
}
