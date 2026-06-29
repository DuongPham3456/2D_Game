using UnityEngine;
using TMPro;

public class EndingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerStats playerStats;

    [Header("UI")]
    public GameObject endingPanel;
    public TextMeshProUGUI endingText;

    void Awake()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
    }

    public void ShowEnding()
    {
        float knowledge = playerStats != null ? playerStats.Knowledge : 0f;
        int debt = playerStats != null ? playerStats.TotalDebt : 0;

        var ending = GameRules.SelectEnding(knowledge, debt);
        float gpa = GameRules.GpaFor(knowledge);

        string body =
            $"{GameRules.TitleOf(ending)}\n\n" +
            $"Final GPA: {gpa:F1}\n" +
            $"Debt: {debt:N0} VND";

        Debug.Log("[Ending] " + body);

        if (endingText != null) endingText.text = body;
        if (endingPanel != null) endingPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}
