using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class TestStore : StoreBase
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public TestStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory) { }

    private IIncludableQueryable<Test, List<WorkerLog>> WithEngineAndWorkerLogs() =>
        Context.Tests
            .Include(t => t.Engine)
            .Include(t => t.WorkerLogs);
    
    private bool AnyRunningTest() 
        => Context.Tests.Any(t => t.State == TestState.Running);
    
    private Test? PausedTestWithHighestPriority() 
        => WithEngineAndWorkerLogs()
            .MaxBy(t => t.Priority);
    
    /// <summary>
    /// Returns a test, that should be run on the worker.
    /// Note, this method also includes <see cref="Test.WorkerLogs" /> and <see cref="Test.Engine" />
    /// Algorithm for finding optimal test to run:
    /// -> if there is not running test, we will pick a paused test with the highest priority.
    /// -> else:
    ///     -> we will find a maximum running priority.
    ///     -> we will find a test that is not running with a same priority.
    ///         -> if test exists, we return.
    ///         -> otherwise we will select test by max (test.ThreadScale / 2 / Test.ActiveWorkerThreadCount())
    /// </summary>
    public Test? GetNextTestForWorker(bool autobench)
    {
        // If there is not running test, we will pick a paused test with the highest priority.
        var anyRunningTest = AnyRunningTest();
        if (!anyRunningTest)
        {
            var test = PausedTestWithHighestPriority();
            return test;
        }
        
        // Get maximum priority of running test.
        var runningPriority = Context.Tests
            .Where(t => t.State == TestState.Running)
            .Max(t => t.Priority);

        // Get a paused test with a same priority 
        var notRunningTestWithoutWorkers = 
            WithEngineAndWorkerLogs()
            .Where(t => t.State == TestState.Paused && t.Priority == runningPriority && t.Autobenched == autobench)
            .OrderByDescending(t => t.ThreadScale)
            .FirstOrDefault();
        
        // If doesn't exist's, select by math.
        var result = notRunningTestWithoutWorkers
                     ?? WithEngineAndWorkerLogs()
                         .Where(t => t.State == TestState.Running && t.Priority == runningPriority && t.Autobenched == autobench)
                         .MaxBy(t => (t.ThreadScale / 2) / 
                                     (t.WorkerLogs
                                         .Where(wl => wl.NumberOfGames != wl.TotalNumberOfGames)
                                         .Sum(wl => wl.NumberOfThreads)
                                     ) 
                                     ); // inlined Test.ActiveWorkerThreadCount()
        
        return result;
    }
    
    /// <summary>
    /// Updates test entity
    /// </summary>
    public void Update(Test test) => Context.Tests.Update(test);

    /// <summary>
    /// Stops a test by an id.
    /// </summary>
    public async Task StopTest(int testId)
        => await Context.Tests
            .Where(t => t.Id == testId)
            .ExecuteUpdateAsync(spc => spc.SetProperty(t => t.State, TestState.Stopped));
}
