using UnityEngine;

public class InteractablePrompt : MonoBehaviour
{
    [Header("Kéo vật thể chữ / nút E vào đây")]
    public GameObject hintUI; 

    // Khi game vừa bắt đầu, đảm bảo chữ đang bị tắt đi
    void Start()
    {
        if (hintUI != null)
        {
            hintUI.SetActive(false);
        }
    }

    // Khi người chơi chạm vào vùng (Collider)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (hintUI != null) hintUI.SetActive(true);
        }
    }

    // Khi người chơi đi ra khỏi vùng
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (hintUI != null) hintUI.SetActive(false);
        }
    }
}