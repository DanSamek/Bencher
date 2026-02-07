using Worker.TestProcessors.GameTestProcessor;

namespace Worker.Tests.TestProcessors.GameTestProcessor;

[TestFixture]
public class TmScalerTests
{
    /// <summary>
    /// Tests <see cref="TmScaler.Scale"/>.
    /// </summary>
    [TestCase(500000, 500000, "5+0.07", 5,0.07)]
    [TestCase(400000, 500000, "5+0.02", 6.25,0.025)]
    [TestCase(1000000, 500000, "5+0.02", 2.5, 0.01)]
    public void Scale(int baseNps, int expectedNps, string timeManagement, decimal expectedSeconds, decimal expectedIncrement)
    {
        var (seconds, increment) = TmScaler.Scale(baseNps, expectedNps, timeManagement);
        Assert.That(expectedSeconds, Is.EqualTo(seconds));
        Assert.That(expectedIncrement, Is.EqualTo(increment));
    }
}