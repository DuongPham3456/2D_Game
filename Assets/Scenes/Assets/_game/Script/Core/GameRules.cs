using UnityEngine;

/// Pure, engine-light game scoring rules. No MonoBehaviour, no scene state —
/// everything here is deterministic and unit-tested.
public static class GameRules
{
    public enum KnowledgeClass { Fail, Average, Good, Excellent }
    public enum Ending { HonorsGraduate, BrilliantButBroke, FreeButAdrift, Crushed }

    public const float ExcellentThreshold = 180f;
    public const float GoodThreshold = 130f;
    public const float AverageThreshold = 70f;

    public const float StressedThreshold = 25f;
    public const float BreakdownRecoverTo = 15f;

    public static KnowledgeClass ClassFor(float knowledge)
    {
        if (knowledge >= ExcellentThreshold) return KnowledgeClass.Excellent;
        if (knowledge >= GoodThreshold) return KnowledgeClass.Good;
        if (knowledge >= AverageThreshold) return KnowledgeClass.Average;
        return KnowledgeClass.Fail;
    }

    public static float GpaFor(float knowledge)
    {
        float gpa = Mathf.Clamp(knowledge, 0f, 200f) / 200f * 4f;
        return Mathf.Round(gpa * 10f) / 10f;
    }

    public static Ending SelectEnding(float knowledge, int debt)
    {
        bool highGpa = knowledge >= ExcellentThreshold;
        bool debtCleared = debt <= 0;
        if (highGpa) return debtCleared ? Ending.HonorsGraduate : Ending.BrilliantButBroke;
        return debtCleared ? Ending.FreeButAdrift : Ending.Crushed;
    }

    public static string TitleOf(Ending e)
    {
        switch (e)
        {
            case Ending.HonorsGraduate: return "Honors Graduate";
            case Ending.BrilliantButBroke: return "Brilliant but Broke";
            case Ending.FreeButAdrift: return "Free but Adrift";
            default: return "Crushed";
        }
    }

    public static float StressedKnowledgeMultiplier(float sanity)
        => sanity < StressedThreshold ? 0.5f : 1f;

    public static float StressedEnergyMultiplier(float sanity)
        => sanity < StressedThreshold ? 1.25f : 1f;
}
