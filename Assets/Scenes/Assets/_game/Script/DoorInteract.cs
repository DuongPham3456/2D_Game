using UnityEngine;

public class DoorInteract : MonoBehaviour
{
    [Header("Giao diện & Âm thanh")]
    public GameObject chuPhimE;       
    public AudioSource doorSound;     

    private bool isPlayerNear = false;
    private bool isDoorOpen = false;  

    void Start()
    {
        if (chuPhimE != null) chuPhimE.SetActive(false);
    }

    void Update()
    {
        // XÓA ĐIỀU KIỆN (!isDoorOpen) để lúc nào đứng gần cũng bấm E được
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            TuongTacCua();
        }
    }

    void TuongTacCua()
    {
        // 1. Đảo ngược trạng thái: đang mở thành đóng, đang đóng thành mở
        isDoorOpen = !isDoorOpen;

        // 2. Phát tiếng động (dùng chung 1 tiếng kẹt cửa)
        if (doorSound != null)
        {
            doorSound.Play();
        }

        // 3. Xử lý Logic hình ảnh (Tàng hình khi mở, hiện lại khi đóng)
        if (isDoorOpen)
        {
            Debug.Log("Cửa đã MỞ!");
            // Làm cửa tàng hình để người chơi đi qua
            GetComponent<SpriteRenderer>().enabled = false;
        }
        else
        {
            Debug.Log("Cửa đã ĐÓNG!");
            // Hiện lại hình ảnh cánh cửa
            GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            
            // LUÔN LUÔN hiện chữ E khi lại gần (dù cửa đang đóng hay mở)
            if (chuPhimE != null)
            {
                chuPhimE.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            
            // Đi xa thì cất chữ E đi
            if (chuPhimE != null)
            {
                chuPhimE.SetActive(false);
            }
        }
    }
}