using UnityEngine;

// Toggle a door open/closed. Interaction (proximity + E + prompt) is handled by the
// ActionTrigger on this same object; we just react to its Interacted event.
[RequireComponent(typeof(ActionTrigger))]
public class DoorInteract : MonoBehaviour
{
    [Header("Âm thanh")]
    public AudioSource doorSound;

    private SpriteRenderer doorSprite;
    private bool isDoorOpen = false;

    void Awake()
    {
        var trigger = GetComponent<ActionTrigger>();
        if (trigger != null) trigger.Interacted += Toggle;
    }

    void Start()
    {
        doorSprite = GetComponent<SpriteRenderer>();
    }

    void Toggle()
    {
        isDoorOpen = !isDoorOpen;
        if (doorSound != null) doorSound.Play();
        // Tàng hình khi mở để đi qua, hiện lại khi đóng
        if (doorSprite != null) doorSprite.enabled = !isDoorOpen;
        Debug.Log(isDoorOpen ? "Cửa đã MỞ!" : "Cửa đã ĐÓNG!");
    }
}
