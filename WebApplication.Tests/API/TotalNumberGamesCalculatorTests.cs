using WebApplication.API;

namespace WebApplication.Tests.API;

[TestFixture]
public class TotalNumberGamesCalculatorTests
{
    /// <summary>
    /// Tests <see cref="TotalNumberGamesCalculator.Calculate"/>. 
    /// </summary>
    [TestCase(8,8,16)]
    [TestCase(16,2,128)]
    [TestCase(10,1,160)]
    [TestCase(3,2,16)]
    [TestCase(64,8,128)]
    [TestCase(64,1,1024)]
    public void Calculate(int numberOfWorkerThreads, int numberOfThreadsTest, int expected)
    {
        var result = TotalNumberGamesCalculator.Calculate(numberOfWorkerThreads, numberOfThreadsTest);
        Assert.That(result,Is.EqualTo(expected));
        Assert.That(result % 2,Is.EqualTo(0));
    }

    /// <summary>
    /// Tests <see cref="TotalNumberGamesCalculator.Calculate"/>.
    /// We expect that method will throw the exception because numberOfWorkerThreads will be smaller than numberOfThreadsTest.
    /// NOTE: this should NOT happen. If so, different logic in the code is incorrect. 
    /// </summary>
    [Test]
    public void Calculate_Throws()
    {
        Assert.Throws<Exception>(() =>  TotalNumberGamesCalculator.Calculate(5, 10));
    }
}