using UnityEngine;
using TMPro;

// Screen popup. Call from anywhere: NotificationManager.Instance?.Show("text");
public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("UI")]
    public GameObject panel;        // the popup container, hidden by default
    public TextMeshProUGUI text;    // the message label

    void Awake() => Instance = this;

    public void Show(string message, float seconds = 3f)
    {
        if (panel == null || text == null) return;
        text.text = message;
        panel.SetActive(true);
        CancelInvoke(nameof(Hide));
        Invoke(nameof(Hide), seconds);
    }

    void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }
}
