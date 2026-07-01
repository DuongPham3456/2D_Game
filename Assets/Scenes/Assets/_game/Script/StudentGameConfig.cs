using UnityEngine;

[CreateAssetMenu(fileName = "StudentGameConfig", menuName = "HUST Student/Game Config")]
public class StudentGameConfig : ScriptableObject
{
    [Header("Starting Stats")]
    public int startingMoney = 200_000;
    public int startingDebt = 3_000_000;
    public float startingEnergy = 100f;
    public float maxEnergy = 100f;
    public float startingSanity = 100f;
    public float maxSanity = 100f;
    public float startingKnowledge = 0f;
    public float maxKnowledge = 200f;


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
    public int dailyLivingCost = 100_000;

    [Header("Quiz Study (Desk) — costs scale with stress like classroom study")]
    public float quizStudyEnergyCost = 20f;
    public float quizExamEnergyCost = 40f;
    public float quizStudySanityCost = 8f;
    public float quizNormalKnowledgeGain = 5f;
    public float quizMidtermKnowledgeGain = 15f;
    public float quizFinalKnowledgeGain = 25f;
}
