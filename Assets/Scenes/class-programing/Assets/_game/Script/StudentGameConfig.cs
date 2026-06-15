using UnityEngine;

[CreateAssetMenu(fileName = "StudentGameConfig", menuName = "HUST Student/Game Config")]
public class StudentGameConfig : ScriptableObject
{
    [Header("Starting Stats")]
    public float startingGpa = 2.0f;
    public int startingMoney = 5_000_000;
    public float startingStamina = 100f;
    public float maxStamina = 100f;
    public int startingDebt = 30_000_000;
    public float maxGpa = 4.0f;

    [Header("Study (Library)")]
    public float studyStaminaCost = 20f;
    public float studyGpaGain = 0.1f;
    public float studyHours = 2f;

    [Header("Work (Cafe)")]
    public float workStaminaCost = 30f;
    public int workMoneyGain = 500_000;
    public float workHours = 4f;

    [Header("Rest (Dorm)")]
    public float restStaminaRestore = 100f;
    public float restHours = 8f;
    public bool restSetsStaminaToMax = true;

    [Header("Daily Living Cost")]
    public int dailyLivingCost = 50_000;
    public bool chargeLivingCostAtEndOfDay = true;
}
