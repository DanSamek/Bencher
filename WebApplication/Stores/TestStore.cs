using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class TestStore : StoreBase
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public TestStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory) { }
    
    /// <summary>
    /// Returns a test, that should be run on the worker.
    /// Note, this method also includes <see cref="Test.WorkerLogs" />, <see cref="Test.Engine" /> and <see cref="Test.AutobenchState" /> 
    /// Algorithm for finding optimal test to run:
    /// NOTE if, test is autobenched (t.Autobenched = true), but it's Resolved, it's not an autobench test anymore, but a normal test.)
    /// And of course we need to find a test, that requires less or equal number of threads to run.
    /// -> if there is not running test, we will pick a paused test with the highest priority.
    /// -> else:
    ///     -> we will find a maximum running priority.
    ///     -> we will find a test that is not running with a same priority.
    ///         -> if test exists, we return.
    ///         -> otherwise we will select test by max ((test.ThreadScale / 2) / Test.ActiveWorkerThreadCount())
    /// TODO ? Change test scale by user's running test count.
    ///
    /// NOTE: when a test is returned from the method, it's automatically set do running.
    /// </summary>
    public Test? GetNextTestForWorker(bool autobench, int workerNumberOfThreads)
    {
        // If there is not running test, we will pick a paused test with the highest priority.
        var anyRunningTest = AnyRunningTest(workerNumberOfThreads);
        if (!anyRunningTest)
        {
            var test = PausedTestWithHighestPriority(autobench, workerNumberOfThreads);
            return test;
        }
        
        // Get maximum priority of running test.
        var runningPriority = Context.Tests
            .Where(t => t.State == TestState.Running)
            .Max(t => t.Priority);

        // Get a paused test with a same priority.
        var notRunningTestWithoutWorkers =
            WhereFilter(Include(), autobench, workerNumberOfThreads)
                .Where(t => t.State == TestState.Paused && t.Priority == runningPriority)
                .OrderByDescending(t => t.ThreadScale)
                .FirstOrDefault();
        
        // If a paused test doesn't exist's, select by math.
        var result = notRunningTestWithoutWorkers
                     ?? WhereFilter(Include(), autobench,  workerNumberOfThreads)
                         .Where(t => t.State == TestState.Running && t.Priority == runningPriority)
                         .OrderByDescending(t => (t.ThreadScale / 2) / (t.WorkerLogs.Where(wl => wl.NumberOfGames != wl.TotalNumberOfGames).Sum(wl => wl.NumberOfThreads))) // inlined Test.ActiveWorkerThreadCount()
                         .FirstOrDefault(); 
        return result;
    }
    
    /// <summary>
    /// Updates test state to <see cref="TestState.Running"/> or <see cref="TestState.Autobenched"/>.
    /// </summary>
    /// <param name="test"></param>
    public void SetRunningState(Test test)
    {
        if (test.State != TestState.Paused) return;
        
        var state = test.AutobenchState is null || test.AutobenchState!.Resolved
            ? TestState.Running : TestState.Autobenched;

        test.State = state;
        Update(test);
        Context.SaveChanges();
    }
    
    /// <summary>
    /// Updates test entity
    /// </summary>
    public void Update(Test test) => Context.Tests.Update(test);
    
    /// <summary>
    /// Stops a test by an id.
    /// </summary>
    public async Task StopTest(int testId)
        => await SetState(testId, TestState.Stopped);
    
    /// <summary>
    /// Sets a state for a test. 
    /// </summary>
    public async Task SetState(int testId, TestState state)
        => await Context.Tests
            .Where(t => t.Id == testId)
            .ExecuteUpdateAsync(spc => spc.SetProperty(t => t.State, state));
    
    /// <summary>
    /// Sets a state for a test. 
    /// </summary>
    public void SetState(Test test, TestState state)
    {
        test.State = state;
        Update(test);
        Context.SaveChanges();
    }
    
    // NOTE: Good for read-only stuff - used .AsNoTracking()
    private IQueryable<Test> Include() =>
        Context.Tests
            .Include(t => t.Engine)
            .Include(t => t.WorkerLogs)
            .Include(t => t.AutobenchState)
            .AsNoTracking(); 
    
    private bool AnyRunningTest(int workerNumberOfThreads) 
        => Context.Tests.Any(t => t.State == TestState.Running && t.NumberOfThreads <= workerNumberOfThreads);
    

    private Test? PausedTestWithHighestPriority(bool autobench, int workerNumberOfThreads)
        => WhereFilter(Include(), autobench,  workerNumberOfThreads)
            .OrderByDescending(t => t.Priority)
            .FirstOrDefault();

    private IQueryable<Test> WhereFilter(IQueryable<Test> tests, bool autobench, int workerNumberOfThreads)
        => tests.Where(t => t.NumberOfThreads <= workerNumberOfThreads &&
                            (!t.Autobenched ? t.Autobenched == autobench : t.AutobenchState!.Resolved == !autobench));
    /*
    private static bool Filter(Test test, bool autobench, int workerNumberOfThreads)
    {
        if (test.NumberOfThreads > workerNumberOfThreads) return false;

        if (!test.Autobenched) return test.Autobenched == autobench;

        // We have autobench test here, autobenchState can't be null!
        return test.AutobenchState!.Resolved == !autobench;
    }*/

}