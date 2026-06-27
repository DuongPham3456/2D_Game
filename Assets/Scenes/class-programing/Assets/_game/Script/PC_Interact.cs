using UnityEngine;

public class PC_Interact : MonoBehaviour
{
    public GameObject canvasPC;
    
    [Header("Giao diện Gợi ý")]
    public GameObject chuPhimE; // Kéo vật thể chữ Chu_PhimE vào đây

    private bool isPlayerNear = false;

    void Start()
    {
        // Vừa vào game là giấu dòng chữ đi luôn
        if (chuPhimE != null) chuPhimE.SetActive(false);
    }

    void Update()
    {
        // Khi đứng gần và bấm E
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            // Bật màn hình PC
            canvasPC.SetActive(true);
            
            // Đang dùng máy tính thì giấu chữ E đi
            if (chuPhimE != null) chuPhimE.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            isPlayerNear = true;
            
            // Chỉ hiện chữ E lên nếu màn hình PC ĐANG TẮT
            if (chuPhimE != null && !canvasPC.activeSelf) 
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
            
            // Đi xa khỏi máy tính thì tắt chữ đi
            if (chuPhimE != null) chuPhimE.SetActive(false);
        }
    }
}