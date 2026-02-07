using Worker.TestProcessors.GameTestProcessor;

namespace Worker.Tests.TestProcessors.GameTestProcessor;

[TestFixture]
public class GamePairCalculatorTests
{
    /// <summary>
    /// Tests <see cref="GamePairCalculator.CalculatePairsNeeded"/>.
    /// </summary>
    [TestCase("8+0.08", 1, 10, 40)]
    [TestCase("8+0.08", 2, 10, 15)]
    [TestCase("5+0.05", 1, 64, 256)]
    [TestCase("5+0.05", 2, 64, 96)]
    [TestCase("5+0.05", 3, 64, 63)]
    [TestCase("60+0.6", 1, 8, 24)]
    [TestCase("120+0.6", 1, 8, 16)]
    [TestCase("120+0.6", 2, 8, 8)]
    public void CalculatePairsNeeded(string timeManagement, int numberOfTestThreads, int availableThreads, int expected)
    {
        var result = GamePairCalculator.CalculatePairsNeeded(timeManagement, numberOfTestThreads, availableThreads);
        Assert.That(expected, Is.EqualTo(result));
    }
}