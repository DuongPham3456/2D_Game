using System.Collections;
using UnityEngine;

public class Counter : MonoBehaviour
{
    public enum CounterType { CupDispenser, CoffeeMachine, Delivery }
    public CounterType type; 

    private bool isPlayerNearby = false;
    private PlayerManager player; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            player = collision.GetComponent<PlayerManager>();
            Debug.Log("Đang đứng gần trạm: " + type + " | Bấm C để tương tác");
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

    void Update()
    {
        // ĐÃ ĐỔI: Sử dụng KeyCode.C thay vì KeyCode.E
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.C) && player != null)
        {
            HandleInteraction();
        }
    }

    void HandleInteraction()
    {
        if (type == CounterType.CupDispenser)
        {
            if (player.myHand == PlayerManager.HandState.Empty)
            {
                player.myHand = PlayerManager.HandState.HasEmptyCup;
                Debug.Log("Đã lấy 1 cái cốc rỗng!");
            }
        }
        else if (type == CounterType.CoffeeMachine)
        {
            if (player.myHand == PlayerManager.HandState.HasEmptyCup)
            {
                player.myHand = PlayerManager.HandState.Empty;
                Debug.Log("Đang pha cà phê... chờ 3 giây");
                StartCoroutine(BrewingRoutine());
            }
        }
        else if (type == CounterType.Delivery)
        {
            if (player.myHand == PlayerManager.HandState.HasCoffeeCup)
            {
                player.myHand = PlayerManager.HandState.Empty;
                player.gold += 50; 
                Debug.Log("Giao hàng thành công! Nhận 50 vàng. Tổng vàng: " + player.gold);
            }
        }
    }

    IEnumerator BrewingRoutine()
    {
        yield return new WaitForSeconds(3f); 
        Debug.Log("Cà phê đã pha xong! Hãy đứng gần máy và bấm C để lấy.");
        
        if (isPlayerNearby && player != null)
        {
            player.myHand = PlayerManager.HandState.HasCoffeeCup;
            Debug.Log("Đã cầm cốc cà phê nóng hổi!");
        }
    }
}