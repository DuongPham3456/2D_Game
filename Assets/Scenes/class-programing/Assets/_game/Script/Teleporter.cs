using UnityEngine;
using TMPro; 

public class Teleport : MonoBehaviour
{
    public Transform destination; 
    public string tenDiaDiem;     
    public GameObject nutE;       
    public TMP_Text textMesh;     

    private bool isPlayerInZone = false;
    private GameObject player;

    void Start()
    {
        // 1. Tự động ghép nối chữ
        if (textMesh != null)
        {
            textMesh.text = "Bấm E để tới " + tenDiaDiem;
        }

        // 2. BẢO HIỂM LỖI: Ép dòng chữ phải tắt đi ngay khi game vừa bật lên!
        if (nutE != null)
        {
            nutE.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.E))
        {
            // Dịch chuyển nhân vật
            player.transform.position = destination.position;
            
            // Xóa chữ đi và reset trạng thái vùng để tránh lỗi
            isPlayerInZone = false; 
            if (nutE != null) nutE.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            player = other.gameObject;
            
            // Hiện chữ lên khi đi vào vùng
            if (nutE != null) nutE.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            player = null;
            
            // Tắt chữ đi khi đi ra khỏi vùng
            if (nutE != null) nutE.SetActive(false);
        }
    }
}