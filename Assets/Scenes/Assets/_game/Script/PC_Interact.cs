using UnityEngine;

// Stand near the bedroom PC and press E to open its menu (PC_Manager lives on canvasPC).
public class PC_Interact : MonoBehaviour
{
    public GameObject canvasPC;

    [Header("Prompt")]
    public GameObject pressEHint;   // "Press E" hint, optional

    bool isPlayerNear;

    void Start()
    {
        if (pressEHint != null) pressEHint.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            if (canvasPC != null) canvasPC.SetActive(true);
            if (pressEHint != null) pressEHint.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNear = true;
        if (pressEHint != null && canvasPC != null && !canvasPC.activeSelf)
            pressEHint.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNear = false;
        if (pressEHint != null) pressEHint.SetActive(false);
    }
}
