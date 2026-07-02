using UnityEngine;
using UnityEngine.EventSystems;

// Show a target object only while the pointer is over this UI element.
// Put on a stat bar; assign its number/label text as the target — it stays hidden
// until you hover the bar. The target should have Raycast Target OFF so it doesn't
// steal the pointer and cause hover flicker.
[RequireComponent(typeof(RectTransform))]
public class HoverReveal : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject target;   // hidden until this element is hovered

    void Start()
    {
        if (target != null) target.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (target != null) target.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (target != null) target.SetActive(false);
    }
}
