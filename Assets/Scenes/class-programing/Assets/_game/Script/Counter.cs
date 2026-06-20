using System.Collections;
using UnityEngine;

public class Counter : MonoBehaviour
{
    public enum CounterType { CupDispenser, CoffeeMachine, Delivery }
    public CounterType type;

    [SerializeField]
    private int deliveryReward = 30000;

    private bool isPlayerNearby = false;
    private PlayerManager player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            player = collision.GetComponent<PlayerManager>();

            Debug.Log("Đang đứng gần quầy: " + type + " | Bấm C để tương tác");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            player = null;
        }
    }

    private void Update()
    {
        if (isPlayerNearby &&
            Input.GetKeyDown(KeyCode.C) &&
            player != null)
        {
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        if (type == CounterType.CupDispenser)
        {
            if (player.myHand == PlayerManager.HandState.Empty)
            {
                player.myHand =
                    PlayerManager.HandState.HasEmptyCup;

                Debug.Log(
                    "Đã lấy 1 cốc rỗng! Hãy đi đến máy pha để lấy café!"
                );
            }
        }
        else if (type == CounterType.CoffeeMachine)
        {
            if (player.myHand == PlayerManager.HandState.HasEmptyCup || player.myHand == PlayerManager.HandState.Empty)
            {
                if (player.myHand == PlayerManager.HandState.HasEmptyCup)
                {
                    player.myHand = PlayerManager.HandState.Empty;
                    Debug.Log("Đang pha café... vui lòng chờ 3 giây!");
                    StartCoroutine(BrewingRoutine());
                }
            }
        }
        else if (type == CounterType.Delivery)
        {
            if (player.myHand ==
                PlayerManager.HandState.HasCoffeeCup)
            {
                PlayerStats stats =
                    player.GetComponent<PlayerStats>();

                if (stats != null)
                {
                    bool success =
                        stats.DeliverCoffee(deliveryReward);

                    if (success)
                    {
                        player.myHand =
                            PlayerManager.HandState.Empty;

                        Debug.Log(
                            "Giao hàng thành công!"
                        );
                    }
                }
                else
                {
                    Debug.LogWarning(
                        "Không tìm thấy component PlayerStats trên Player!"
                    );
                }
            }
        }
    }

    private IEnumerator BrewingRoutine()
    {
    //Chờ 3 giây để pha café
    yield return new WaitForSeconds(3f);

    Debug.Log("Café đã pha xong! Hãy đứng gần máy pha và bấm C để lấy!");

    yield return new WaitUntil(() => isPlayerNearby && player != null && Input.GetKeyDown(KeyCode.C));

    player.myHand = PlayerManager.HandState.HasCoffeeCup;
    Debug.Log("Đã lấy cốc café nóng hổi! Mau giao cho khách thôi nào!");
    }
}

