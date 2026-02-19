using Shared.Dtos.Requests;
using WebApplication.Data.Models;
using WebApplication.Tests.Builders;

namespace WebApplication.Tests.API;

[TestFixture]
public class TestServiceTests : WorkerControllerTestBase
{
    /// <inheritdoc /> 
    public override void Setup()
    {
        base.Setup();
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test_book")
            .CreateSprtSettings(0, 5)
            .CreateUser("test_user")
            .WithAccessToken("ABCDEF")
            .AddEngine("stockfish")
            .AddBranch("base_branch")
            .AddBranch("test_branch")
            .AddTest("test_1", "test_book", "base_branch", "test_branch")
            .EnsurePentaCreated(Factory.CreateDbContext())
            .Close()
            .Close()
            .Close()
            .Close();
    }

    /// <summary>
    /// This test tests iterated updates - UpdateSPRTResults.
    /// We expect when the test passes, test is in the finished state and also workers are in the finished state.
    /// Our workers has 6 threads -> 128 games per run
    /// </summary>
    [Test]
    public async Task Iterated_UpdateSPRTResults()
    {
        var accessToken = Factory
            .CreateDbContext()
            .Users
            .First(u => u.UserName == "test_user")
            .AccessToken!;
        
        var pentaUpdates = new List<ResultsDto>();
        // 32 games per update - 4 per worker log
        for (var i = 0; i < 10; i++)
        {
            AddPentaUpdates();
        }
        
        var total = pentaUpdates.Count / 4;
        var j = 0;
        var jobs = new List<(WorkerLog, int TestId)>();
        for (var i = 0; i < total; i++)
        {
            var (wl, test) = await CreateTestService()
                .CreateJobForWorker(new GetTestDto
            {
                Autobench = false,
                Mac = "AB:CD:EF:GH:IJ",
                Name = "TEST-WORKER",
                NumberOfThreads = 8
            }, accessToken);
            
            jobs.Add((wl, test.Id));
        }

        var hit = false;
        var passedPentas = new List<ResultsDto>();
        foreach (var (wl, testId) in jobs)
        {
            for (var i = 0; i < 4; i++)
            {
                var penta = pentaUpdates[j];
                penta.ConnectionId = wl.Id;
                var success = await CreateTestService()
                    .UpdateSPRTResults(pentaUpdates[j++]);
                if (hit)
                {
                    Assert.That(success, Is.EqualTo(false));
                    continue;
                }
                passedPentas.Add(penta);

                var updatedTest = CreateTestStore()
                    .GetById(testId)!;
            
                var result = SPRT.Sprt.GetStatistics(updatedTest);
                if (result.Result == SPRT.Sprt.SprtResult.Unknown) continue;

                Assert.That(updatedTest.State, Is.EqualTo(TestState.Finished));
            
                var workerLogs = Factory
                    .CreateDbContext()
                    .WorkerLogs
                    .Where(w => w.Test.Id == updatedTest.Id && w.State == WorkerLogState.Active)
                    .ToArray();
            
                Assert.That(workerLogs, Has.Length.EqualTo(0));
                hit = true;
            }
        }
        Assert.That(hit, Is.EqualTo(true));
        
        var testPenta = CreateTestStore()
            .GetById(jobs[0].TestId)!
            .Penta;
        
        Assert.That(testPenta.Ll, Is.EqualTo(passedPentas.Sum(p => p.Ll)));
        Assert.That(testPenta.Ld, Is.EqualTo(passedPentas.Sum(p => p.Ld)));
        Assert.That(testPenta.Dd, Is.EqualTo(passedPentas.Sum(p => p.Dd)));
        Assert.That(testPenta.Wl, Is.EqualTo(passedPentas.Sum(p => p.Wl)));
        Assert.That(testPenta.Wd, Is.EqualTo(passedPentas.Sum(p => p.Wd)));
        Assert.That(testPenta.Ww, Is.EqualTo(passedPentas.Sum(p => p.Ww)));
        
        return;
        void AddPentaUpdates()
        {
            pentaUpdates.Add(new ResultsDto {Dd = 0, Ll = 0, Ld = 0, Wl = 1, Wd = 5, Ww = 10, ConnectionId = 0 });
            pentaUpdates.Add(new ResultsDto {Dd = 0, Ll = 0, Ld = 0, Wl = 1, Wd = 5, Ww = 10, ConnectionId = 0 });
            pentaUpdates.Add(new ResultsDto {Dd = 0, Ll = 0, Ld = 0, Wl = 6, Wd = 5, Ww = 5, ConnectionId = 0 });
            pentaUpdates.Add(new ResultsDto {Dd = 10, Ll = 0, Ld = 0, Wl = 5, Wd = 1, Ww = 0, ConnectionId = 0 });
        }
    } 
}