using System.Collections;
using TMPro;
using UnityEngine;

// One cafe counter (cup dispenser / coffee machine / delivery). Press C while nearby.
// Only works while a CafeShift is Active. Loop: get cup -> brew -> deliver -> money.
public class Counter : MonoBehaviour
{
    public enum CounterType { CupDispenser, CoffeeMachine, Delivery }
    public CounterType type;

    [SerializeField] int deliveryReward = 15000;   // ~20 cups in a 5-min shift = ~300k
    [SerializeField] float brewSeconds = 3f;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private string cupDispenserHint = "Press C to get an empty cup";
    [SerializeField] private string brewCoffeeHint = "Press C to brew coffee";
    [SerializeField] private string pickCoffeeCupHint = "Press C to pick up the coffee cup";
    [SerializeField] private string deliveryHint = "Press C to deliver coffee";
    [SerializeField] private string shiftHint = "Press C to start your shift";

    PlayerStats stats;
    CafeShiftManager shift;
    PlayerManager player;
    bool isPlayerNearby;
    bool coffeeReadyForPickup;
    bool isBrewing;

    void Awake()
    {
        // PlayerStats lives on the game manager, not the player, so find it directly.
        stats = FindFirstObjectByType<PlayerStats>();
        shift = FindFirstObjectByType<CafeShiftManager>();
    }

    void Start()
    {
        if (hintText == null)
            hintText = GetComponentInChildren<TextMeshProUGUI>(true);

        SetHintVisible(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNearby = true;
        player = other.GetComponent<PlayerManager>();
        UpdateHintText();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || player == null) return;
        UpdateHintText();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNearby = false;
        player = null;
        SetHintVisible(false);
    }

    void Update()
    {
        if (UIModal.IsOpen)
        {
            SetHintVisible(false);
            return;
        }

        UpdateHintText();

        if (isPlayerNearby && player != null && Input.GetKeyDown(KeyCode.C))
            HandleInteraction();
    }

    void UpdateHintText()
    {
        if (!isPlayerNearby || player == null)
        {
            SetHintVisible(false);
            return;
        }

        if (shift != null && !shift.Active)
        {
            SetHintText(shiftHint);
            return;
        }

        switch (type)
        {
            case CounterType.CupDispenser:
                SetHintText(player.myHand == PlayerManager.HandState.Empty ? cupDispenserHint : "");
                break;

            case CounterType.CoffeeMachine:
                if (isBrewing)
                {
                    SetHintVisible(false);
                }
                else if (player.myHand == PlayerManager.HandState.HasEmptyCup)
                {
                    SetHintText(brewCoffeeHint);
                }
                else if (player.myHand == PlayerManager.HandState.Empty && coffeeReadyForPickup)
                {
                    SetHintText(pickCoffeeCupHint);
                }
                else
                {
                    SetHintVisible(false);
                }
                break;

            case CounterType.Delivery:
                SetHintText(player.myHand == PlayerManager.HandState.HasCoffeeCup ? deliveryHint : "");
                break;

            default:
                SetHintVisible(false);
                break;
        }
    }

    public void ResetForNewCycle()
    {
        coffeeReadyForPickup = false;
        isBrewing = false;
        UpdateHintText();
    }

    void SetHintText(string text)
    {
        if (hintText == null) return;

        hintText.text = text;
        hintText.gameObject.SetActive(!string.IsNullOrEmpty(text));
    }

    void SetHintVisible(bool visible)
    {
        if (hintText == null) return;
        hintText.gameObject.SetActive(visible);
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
                    coffeeReadyForPickup = false;
                    isBrewing = false;
                    Debug.Log("Picked up an empty cup. Go to the coffee machine.");
                }
                break;

            case CounterType.CoffeeMachine:
                if (player.myHand == PlayerManager.HandState.HasEmptyCup)
                {
                    player.myHand = PlayerManager.HandState.Empty;
                    coffeeReadyForPickup = false;
                    isBrewing = true;
                    Debug.Log("Brewing... wait a moment.");
                    StartCoroutine(Brew());
                }
                else if (coffeeReadyForPickup && player.myHand == PlayerManager.HandState.Empty)
                {
                    player.myHand = PlayerManager.HandState.HasCoffeeCup;
                    coffeeReadyForPickup = false;
                    isBrewing = false;
                    Debug.Log("Got the hot coffee! Deliver it.");
                }
                break;

            case CounterType.Delivery:
                if (player.myHand == PlayerManager.HandState.HasCoffeeCup)
                {
                    if (stats != null && stats.DeliverCoffee(deliveryReward))
                    {
                        player.myHand = PlayerManager.HandState.Empty;

                        Counter[] allCounters = FindObjectsOfType<Counter>();
                        foreach (Counter counter in allCounters)
                        {
                            counter.ResetForNewCycle();
                        }
                    }
                }
                break;
        }

        StartCoroutine(RefreshHintNextFrame());
    }

    IEnumerator Brew()
    {
        SetHintVisible(false);
        yield return new WaitForSeconds(brewSeconds);
        coffeeReadyForPickup = true;
        isBrewing = false;
        Debug.Log("Coffee ready! Press C at the machine to pick it up.");
        StartCoroutine(RefreshHintNextFrame());
    }

    IEnumerator RefreshHintNextFrame()
    {
        yield return null;
        UpdateHintText();
    }
}
