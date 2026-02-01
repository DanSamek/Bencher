using WebApplication.Data.Models;

namespace WebApplication.Tests.Sprt;

[TestFixture]
public class SprtTests
{
    [TestCase(0,0,0,0,0,0, 0.0, SPRT.Sprt.SprtResult.Unknown)]
    [TestCase(0,0,0,1,0,0, 0.0, SPRT.Sprt.SprtResult.Unknown)]
    [TestCase(83, 3119, 6657, 0,2916, 73,-2.95, SPRT.Sprt.SprtResult.H0NotRejected)]
    [TestCase(1725, 64545, 140558,0, 65564, 1752,2.97, SPRT.Sprt.SprtResult.H0Rejected)]
    [TestCase(202, 7621, 0, 16862, 7846, 221,1.87, SPRT.Sprt.SprtResult.Unknown)]
    public void WithPentaData(int ll, int ld, int dd, int wl, int wd, int ww, double expectedApproxLlr, SPRT.Sprt.SprtResult expectedResult)
    {
        var test = new Test
        {
            Settings = new SprtSettings
            {
                Elo0 = 0,
                Elo1 = 2,
                Alpha = 0.05,
                Beta = 0.05,
                Tests = null!
            },
            Name = null!,
            Created = default,
            Priority = 0,
            NumberOfThreads = 0,
            HashSize = 0,
            TimeManagement = null!,
            State = TestState.Paused,
            OpeningBook = null!,
            Errors = null!, 
            WorkerLogs = null!,
            Engine = null!,
            User = null!,
            Autobenched = false,
            ExpectedNps = 0,
            Penta = new Penta
            {
                Test = null!,
                Ll = ll,
                Ld = ld,
                Dd = dd,
                Wl = wl,
                Wd = wd,
                Ww = ww,
            }
        };
        
        var statistics = SPRT.Sprt.GetStatistics(test);
        Assert.That(double.Round(statistics.Llr, 2), Is.EqualTo(expectedApproxLlr)); // TODO!
        Assert.That(statistics.Result, Is.EqualTo(expectedResult));
    }
}