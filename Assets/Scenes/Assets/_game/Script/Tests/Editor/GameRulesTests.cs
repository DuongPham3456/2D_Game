using NUnit.Framework;

public class GameRulesTests
{
    [TestCase(69f, GameRules.KnowledgeClass.Fail)]
    [TestCase(70f, GameRules.KnowledgeClass.Average)]
    [TestCase(129f, GameRules.KnowledgeClass.Average)]
    [TestCase(130f, GameRules.KnowledgeClass.Good)]
    [TestCase(179f, GameRules.KnowledgeClass.Good)]
    [TestCase(180f, GameRules.KnowledgeClass.Excellent)]
    [TestCase(200f, GameRules.KnowledgeClass.Excellent)]
    public void ClassFor_RespectsThresholds(float knowledge, GameRules.KnowledgeClass expected)
    {
        Assert.AreEqual(expected, GameRules.ClassFor(knowledge));
    }

    [TestCase(0f, 0f)]
    [TestCase(100f, 2f)]
    [TestCase(180f, 3.6f)]
    [TestCase(200f, 4f)]
    public void GpaFor_IsLinearAndRounded(float knowledge, float expected)
    {
        Assert.AreEqual(expected, GameRules.GpaFor(knowledge), 0.001f);
    }

    [TestCase(180, 0, GameRules.Ending.HonorsGraduate)]
    [TestCase(180, 5, GameRules.Ending.BrilliantButBroke)]
    [TestCase(120, 0, GameRules.Ending.FreeButAdrift)]
    [TestCase(120, 5, GameRules.Ending.Crushed)]
    public void SelectEnding_PicksCorrectCell(float knowledge, int debt, GameRules.Ending expected)
    {
        Assert.AreEqual(expected, GameRules.SelectEnding(knowledge, debt));
    }

    [Test]
    public void StressedMultipliers_KickInBelow25()
    {
        Assert.AreEqual(1f, GameRules.StressedKnowledgeMultiplier(25f), 0.001f);
        Assert.AreEqual(0.5f, GameRules.StressedKnowledgeMultiplier(24f), 0.001f);
        Assert.AreEqual(1f, GameRules.StressedEnergyMultiplier(25f), 0.001f);
        Assert.AreEqual(1.25f, GameRules.StressedEnergyMultiplier(24f), 0.001f);
    }
}
