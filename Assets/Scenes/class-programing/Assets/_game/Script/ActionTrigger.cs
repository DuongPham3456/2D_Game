using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ActionTrigger : MonoBehaviour
{
    public enum StationType { Study, Work, PCStudy, Entertain, Sleep, Custom }

    [Header("Station")]
    public StationType stationType = StationType.Custom;
    public string promptMessage = "Press E to interact";

    [Header("Events")]
    public UnityEvent onInteract;

    [Header("Optional UI")]
    public TextMeshProUGUI promptText;

    bool isPlayerInRange;

    void Update()
    {
        // Nhận phím E từ cả Input cũ lẫn Input System mới
        bool pressE = Input.GetKeyDown(KeyCode.E);
        
        #if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
        {
            pressE = true;
        }
        #endif
        
        if (pressE && isPlayerInRange)
        {
            onInteract.Invoke();
        }
        
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInRange = true;
        ShowPrompt(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInRange = false;
        ShowPrompt(false);
    }

    void ShowPrompt(bool visible)
    {
        if (promptText == null) return;

        if (visible)
        {
            string label = stationType switch
            {
                StationType.Study => "Press E to Study",
                StationType.Work => "Press E to Work at Cafe",
                StationType.PCStudy => "Press E to Study on PC",
                StationType.Entertain => "Press E to Relax",
                StationType.Sleep => "Press E to Sleep",
                _ => promptMessage
            };
            promptText.text = label;
            promptText.gameObject.SetActive(true);
        }
        else
        {
            promptText.gameObject.SetActive(false);
        }
    }
}
