using UnityEngine;

[CreateAssetMenu(fileName = "StudentGameConfig", menuName = "HUST Student/Game Config")]
public class StudentGameConfig : ScriptableObject
{
    [Header("Starting Stats")]
    public int startingMoney = 5_000_000;
    public int startingDebt = 30_000_000;
    public float startingEnergy = 100f;
    public float maxEnergy = 100f;
    public float startingSanity = 100f;
    public float maxSanity = 100f;
    public float startingKnowledge = 0f;
    public float maxKnowledge = 200f;

    [Header("Classroom Study")]
    public float classStudyEnergyCost = 15f;
    public float classStudySanityCost = 10f;
    public float classStudyKnowledgeGain = 12f;

    [Header("Work (Cafe) — flat pay until mini-game ships")]
    public float workEnergyCost = 20f;
    public float workSanityCost = 8f;
    public int workMoneyGain = 500_000;

    [Header("PC Study (Bedroom)")]
    public float pcStudyEnergyCost = 10f;
    public float pcStudySanityCost = 5f;
    public float pcStudyKnowledgeGain = 6f;

    [Header("Entertain (Bedroom)")]
    public float entertainEnergyCost = 2f;
    public float entertainSanityGain = 15f;

    [Header("Sleep (partial restore)")]
    public float sleepEnergyRestore = 40f;
    public float sleepSanityRestore = 10f;

    [Header("Daily Living Cost")]
    public int dailyLivingCost = 50_000;
}
