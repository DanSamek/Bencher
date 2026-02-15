using WebApplication.Data.Models;

namespace WebApplication.Tests.Data;

[TestFixture]
public class AutobenchStateTests
{
    /// <summary>
    /// Tests <see cref="AutobenchState.IsConfidenceInInterval"/>.
    /// </summary>
    [TestCase(0, false)]
    [TestCase(0.1, true)]
    [TestCase(0.01, true)]
    [TestCase(0.001, true)]
    [TestCase(0.0001, true)]
    [TestCase(0.00009, false)]
    [TestCase(0.000001, false)]
    [TestCase(1, true)]
    [TestCase(1.0001, false)]
    [TestCase(100, false)]
    [TestCase(-100, false)]
    public void IsConfidenceInInterval(double value, bool expected)
    {
        var result = AutobenchState.IsConfidenceInInterval(value);
        Assert.That(result, Is.EqualTo(expected));
    }
}