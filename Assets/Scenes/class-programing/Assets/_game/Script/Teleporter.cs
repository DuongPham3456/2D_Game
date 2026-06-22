using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class Teleport : MonoBehaviour
{
    [System.Serializable]
    public class Destination
    {
        public string name = "Phòng";
        public Transform point;          // nơi nhân vật dịch chuyển tới
        public Transform cameraTarget;   // tùy chọn: camera nhảy theo (để 1 màn 1 phòng)
    }

    [Header("Danh sách điểm đến")]
    public List<Destination> destinations = new List<Destination>();

    [Header("UI")]
    public GameObject nutE;             // gợi ý "Bấm E" khi đứng trong vùng
    public TMP_Text textMesh;           // hiện danh sách điểm đến
    public string promptMessage = "Bấm E để di chuyển";

    bool inZone;
    bool menuOpen;
    GameObject player;

    void Start()
    {
        if (nutE != null) nutE.SetActive(false);
        if (textMesh != null) textMesh.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!inZone) return;

        if (!menuOpen)
        {
            if (Input.GetKeyDown(KeyCode.E)) OpenMenu();
            return;
        }

        // Menu đang mở: E hoặc Esc để hủy
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
        {
            CloseMenu();
            return;
        }

        // Phím số 1..9 -> chọn điểm đến tương ứng (giới hạn 9)
        int max = Mathf.Min(destinations.Count, 9);
        for (int i = 0; i < max; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                TeleportTo(destinations[i]);
                CloseMenu();
                return;
            }
        }
    }

    void OpenMenu()
    {
        menuOpen = true;
        if (nutE != null) nutE.SetActive(false);
        if (textMesh == null) return;

        var sb = new StringBuilder("--- Di chuyển ---\n");
        int max = Mathf.Min(destinations.Count, 9);
        for (int i = 0; i < max; i++)
            sb.Append($"[{i + 1}] {destinations[i].name}\n");
        sb.Append("[Esc] Hủy");

        textMesh.text = sb.ToString();
        textMesh.gameObject.SetActive(true);
    }

    void CloseMenu()
    {
        menuOpen = false;
        if (textMesh != null) textMesh.gameObject.SetActive(false);
        if (nutE != null) nutE.SetActive(inZone);
    }

    void TeleportTo(Destination d)
    {
        if (player == null || d.point == null) return;

        player.transform.position = d.point.position;

        if (d.cameraTarget != null)
        {
            var cam = Camera.main;
            if (cam != null)
                cam.transform.position = new Vector3(
                    d.cameraTarget.position.x,
                    d.cameraTarget.position.y,
                    cam.transform.position.z);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        inZone = true;
        player = other.gameObject;
        if (nutE != null) nutE.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        inZone = false;
        player = null;
        CloseMenu();
    }
}
