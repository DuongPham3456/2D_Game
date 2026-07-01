using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A cafe customer waiting for coffee. 
// 3 Phases: Walk In -> Wait -> Leave
public class CafeCustomer : MonoBehaviour
{
    [SerializeField] int reward = 15000;
    [SerializeField] float walkSpeed = 3f;   // Tốc độ đi vào quầy
    [SerializeField] float leaveSpeed = 6f;  // Tốc độ bỏ đi sau khi mua xong
    [SerializeField] Animator anim;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string promptMessage = "Press E to deliver and make the coffee again";
    [SerializeField] private string promptTextPath = "Canvas/customer";
    private Canvas promptCanvas;
    private RectTransform promptRect;

    CafeShiftManager shift;
    PlayerStats stats;
    PlayerManager player;
    
    bool isPlayerNearby;
    
    // Quản lý 3 trạng thái: 0 = Đang đi vào, 1 = Đang chờ, 2 = Đang rời đi
    int currentPhase = 0; 
    Vector2 targetWaitPos;

    void Awake()
    {
        shift = FindFirstObjectByType<CafeShiftManager>();
        stats = FindFirstObjectByType<PlayerStats>();
        if (anim == null) anim = GetComponent<Animator>();
    }

    void Start()
    {
        FindPromptText();
        SetPromptVisible(false);
    }

    // ĐÂY LÀ HÀM BỊ THIẾU MÀ SPAWNER ĐANG TÌM KIẾM
    public void WalkInTo(Vector2 targetPos)
    {
        targetWaitPos = targetPos;
        currentPhase = 0; // Bắt đầu ở Giai đoạn Đi vào
        if (anim != null) anim.SetBool("isWalking", true);
        
        // Lật mặt nhân vật hướng về phía quầy
        FlipSprite(targetWaitPos.x - transform.position.x); 
    }

    void Update()
    {
        // GIAI ĐOẠN 0: ĐI TỪ CỬA VÀO QUẦY
        if (currentPhase == 0) 
        {
            // Trượt dần tới Wait Point
            transform.position = Vector2.MoveTowards(transform.position, targetWaitPos, walkSpeed * Time.deltaTime);
            
            // Nếu đã tới nơi
            if (Vector2.Distance(transform.position, targetWaitPos) < 0.05f)
            {
                transform.position = targetWaitPos; // Ép khớp vị trí
                currentPhase = 1;                   // Chuyển sang đứng chờ
                if (anim != null) anim.SetBool("isWalking", false);
            }
        }
        // GIAI ĐOẠN 1: ĐỨNG CHỜ PHỤC VỤ
        else if (currentPhase == 1)
        {
            if (UIModal.IsOpen)
            {
                SetPromptVisible(false);
                return;
            }

            if (isPlayerNearby && player != null)
            {
                SetPromptVisible(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    TryServe();
                }
            }
            else
            {
                SetPromptVisible(false);
            }
        }
        // GIAI ĐOẠN 2: RỜI ĐI
        else if (currentPhase == 2)
        {
            // Đi thẳng sang bên phải màn hình
            transform.Translate(Vector2.right * leaveSpeed * Time.deltaTime);
        }
    }

    void TryServe()
    {
        if (shift != null && !shift.Active)
        {
            Debug.Log("[Cafe] Clock in for a shift before serving customers.");
            return;
        }
        if (player.myHand != PlayerManager.HandState.HasCoffeeCup)
        {
            Debug.Log("[Cafe] This customer wants a coffee — brew one and bring it over.");
            return;
        }

        if (stats != null && stats.DeliverCoffee(reward))
        {
            player.myHand = PlayerManager.HandState.Empty;
            
            currentPhase = 2; // Chuyển sang rời đi
            SetPromptVisible(false);
            if (anim != null) anim.SetBool("isWalking", true);
            
            FlipSprite(1f); // Ép quay mặt sang phải khi đi ra
            
            Debug.Log("[Cafe] Coffee handed to the customer! They leave happy.");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Nếu đang rời đi (phase 2) mà đụng trúng bức tường tàng hình -> Biến mất
        if (currentPhase == 2 && other.CompareTag("Wall")) 
        { 
            Destroy(gameObject); 
            return; 
        }
        
        if (!other.CompareTag("Player")) return;

        isPlayerNearby = true;
        player = other.GetComponent<PlayerManager>();
        if (currentPhase == 1)
            SetPromptVisible(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNearby = false;
        player = null;
        SetPromptVisible(false);
    }

    void FindPromptText()
    {
        if (promptText != null) return;

        Transform promptTransform = null;
        if (!string.IsNullOrEmpty(promptTextPath))
        {
            promptTransform = transform.root.Find(promptTextPath);
        }

        if (promptTransform != null)
        {
            promptCanvas = promptTransform.GetComponentInParent<Canvas>();
        }

        if (promptCanvas == null)
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            if (canvases != null && canvases.Length > 0)
            {
                promptCanvas = canvases[0];
            }
        }

        if (promptCanvas != null)
        {
            CreatePromptTextObject();
        }
        else
        {
            CreateCanvasAndPrompt();
        }
    }

    void CreateCanvasAndPrompt()
    {
        GameObject canvasObject = new GameObject("CustomerPromptCanvas", typeof(RectTransform));
        promptCanvas = canvasObject.AddComponent<Canvas>();
        promptCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        promptCanvas.sortingOrder = 1000;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
        CreatePromptTextObject();
    }

    void CreatePromptTextObject()
    {
        if (promptCanvas == null) return;

        if (promptText != null)
        {
            promptText.transform.SetParent(promptCanvas.transform, false);
            promptText.gameObject.SetActive(true);
            promptRect = promptText.rectTransform;
            promptText.raycastTarget = false;
            return;
        }

        GameObject promptObject = new GameObject("CustomerPrompt" + GetInstanceID(), typeof(RectTransform));
        promptObject.transform.SetParent(promptCanvas.transform, false);
        promptObject.layer = LayerMask.NameToLayer("UI");

        promptText = promptObject.AddComponent<TextMeshProUGUI>();
        promptText.text = promptMessage;
        promptText.fontSize = 24;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = Color.white;
        promptText.raycastTarget = false;
        promptText.enableWordWrapping = false;

        promptRect = promptText.rectTransform;
        promptRect.anchorMin = new Vector2(0.5f, 0.5f);
        promptRect.anchorMax = new Vector2(0.5f, 0.5f);
        promptRect.pivot = new Vector2(0.5f, 0.5f);
        promptRect.sizeDelta = new Vector2(320f, 40f);
        promptRect.anchoredPosition = new Vector2(0f, 80f);
        promptRect.localScale = Vector3.one;
    }

    void SetPromptVisible(bool visible)
    {
        if (promptText == null) return;
        promptText.text = promptMessage;
        promptText.gameObject.SetActive(visible);

        if (promptRect != null && visible && promptCanvas != null)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 worldPos = cam.WorldToScreenPoint(transform.position + Vector3.up * 1.4f);
            promptRect.position = worldPos;
        }
    }

    // Hàm tự động lật ảnh (tránh đi lùi)
    void FlipSprite(float directionX)
    {
        // Ghi chú: Nếu bức ảnh gốc của bạn đang vẽ nhìn sang PHẢI:
        // Đoạn code dưới đây hoạt động chuẩn.
        // Nếu ảnh gốc vẽ nhìn sang TRÁI, hãy đổi dấu Mathf.Abs ở dòng dưới (âm thành dương, dương thành âm).
        if (directionX > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (directionX < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
}