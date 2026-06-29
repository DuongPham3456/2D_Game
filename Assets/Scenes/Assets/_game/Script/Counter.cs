using System.Collections;
using UnityEngine;

// One cafe counter (cup dispenser / coffee machine / delivery). Press C while nearby.
// Only works while a CafeShift is Active. Loop: get cup -> brew -> deliver -> money.
public class Counter : MonoBehaviour
{
    public enum CounterType { CupDispenser, CoffeeMachine, Delivery }
    public CounterType type;

    [SerializeField] int deliveryReward = 30000;
    [SerializeField] float brewSeconds = 3f;

    PlayerStats stats;
    CafeShiftManager shift;
    PlayerManager player;
    bool isPlayerNearby;

    void Awake()
    {
        // PlayerStats lives on the game manager, not the player, so find it directly.
        stats = FindFirstObjectByType<PlayerStats>();
        shift = FindFirstObjectByType<CafeShiftManager>();
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

    void Update()
    {
        if (UIModal.IsOpen) return;
        if (isPlayerNearby && player != null && Input.GetKeyDown(KeyCode.C))
            HandleInteraction();
    }

    void HandleInteraction()
    {
        if (shift != null && !shift.Active)
        {
            Debug.Log("Clock in for a cafe shift first (press E at the counter).");
            return;
        }

        switch (type)
        {
            case CounterType.CupDispenser:
                if (player.myHand == PlayerManager.HandState.Empty)
                {
                    player.myHand = PlayerManager.HandState.HasEmptyCup;
                    Debug.Log("Picked up an empty cup. Go to the coffee machine.");
                }
                break;

            case CounterType.CoffeeMachine:
                if (player.myHand == PlayerManager.HandState.HasEmptyCup)
                {
                    player.myHand = PlayerManager.HandState.Empty;
                    Debug.Log("Brewing... wait a moment.");
                    StartCoroutine(Brew());
                }
                break;

            case CounterType.Delivery:
                if (player.myHand == PlayerManager.HandState.HasCoffeeCup)
                {
                    if (stats != null && stats.DeliverCoffee(deliveryReward))
                        player.myHand = PlayerManager.HandState.Empty;
                }
                break;
        }
    }

    IEnumerator Brew()
    {
        yield return new WaitForSeconds(brewSeconds);
        Debug.Log("Coffee ready! Press C at the machine to pick it up.");
        yield return new WaitUntil(
            () => isPlayerNearby && player != null && Input.GetKeyDown(KeyCode.C));
        player.myHand = PlayerManager.HandState.HasCoffeeCup;
        Debug.Log("Got the hot coffee! Deliver it.");
    }
}
