using UnityEngine;

// Trade money for energy (no time slot). Interaction is handled by the ActionTrigger
// on this object (press E); one component per item with its own cost/gain.
[RequireComponent(typeof(ActionTrigger))]
public class VendingMachine : MonoBehaviour
{
    [SerializeField] string itemName = "Energy Drink";
    [SerializeField] int cost = 250000;     // 16.7k/energy — above coffee's 15k, so no money loop
    [SerializeField] float energyGain = 15f;

    PlayerStats stats;

    void Awake()
    {
        stats = FindFirstObjectByType<PlayerStats>();
        var trigger = GetComponent<ActionTrigger>();
        if (trigger != null) trigger.Interacted += Buy;
    }

    void Buy() => stats?.BuyEnergy(cost, energyGain, itemName);
}
