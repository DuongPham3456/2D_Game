using UnityEngine;

// Stand near and press the buy key to trade money for energy. No time slot — a quick
// buy. Put one component per item (energy drink, meal) with its own cost/gain.
public class VendingMachine : MonoBehaviour
{
    [SerializeField] string itemName = "Energy Drink";
    [SerializeField] int cost = 250000;     // 16.7k/energy — above coffee's 15k, so no money loop
    [SerializeField] float energyGain = 15f;
    [SerializeField] KeyCode buyKey = KeyCode.E;

    PlayerStats stats;
    bool isPlayerNear;

    void Awake() => stats = FindFirstObjectByType<PlayerStats>();

    void Update()
    {
        if (UIModal.IsOpen) return;
        if (isPlayerNear && Input.GetKeyDown(buyKey))
            stats?.BuyEnergy(cost, energyGain, itemName);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNear = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNear = false;
    }
}
