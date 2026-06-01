using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("Điểm nhân vật sẽ tới")]
    public Transform destination; 

    // Hàm này tự động chạy khi nhân vật dẫm vào khu vực cửa
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Đổi vị trí của Player sang vị trí Điểm đến
            other.transform.position = destination.position;
            Debug.Log("Vù ù ù... Đã dịch chuyển!");
        }
    }
}