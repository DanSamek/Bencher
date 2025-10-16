using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class TestStore : Store<Test>
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public TestStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory) { }
    
    /// <inheritdoc /> 
    protected override DbSet<Test> GetDbSet() => Context.Tests;
    
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
    /// NOTE: when a test is returned from the method, it's automatically set do running.
    /// </summary>
    public Test? GetNextTestForWorker(bool autobench, int workerNumberOfThreads)
    {
        
        // If there is not running test, we will pick a paused test with the highest priority.
        var anyRunningTest = AnyRunningTest(autobench);
        if (!anyRunningTest)
        {
            var test = PausedTestWithHighestPriority(autobench, workerNumberOfThreads);
            return test;
        }
        
        // Get maximum priority of running test.
        var runningPriority = Context.Tests
            .Where(t => autobench ? t.State == TestState.Autobenched : t.State == TestState.Running)
            .Max(t => t.Priority);

        // Get a paused test with a same priority.
        var notRunningTestWithoutWorkers =
            WhereFilter(Include(), autobench, workerNumberOfThreads)
                .Where(t => t.State == TestState.Paused && t.Priority == runningPriority)
                .OrderByDescending(t => t.ThreadScale)
                .FirstOrDefault();


        if (notRunningTestWithoutWorkers is not null)
        {
            return notRunningTestWithoutWorkers;
        }


        var filteredRunningTests = Include()
            .Where(t => (autobench ? t.State == TestState.Autobenched : t.State == TestState.Running) &&
                        t.Priority == runningPriority);
        
        if (autobench)
        {
            filteredRunningTests = filteredRunningTests
                .OrderByDescending(t => t.ThreadScale);
        }
        else
        {
            filteredRunningTests = filteredRunningTests
                .OrderByDescending(t =>
                    (t.ThreadScale / 2) / (t.WorkerLogs.Where(wl => wl.NumberOfGames != wl.TotalNumberOfGames)
                        .Sum(wl => wl.NumberOfThreads))); // inlined Test.ActiveWorkerThreadCount() 

        }
        
        var result = filteredRunningTests.FirstOrDefault();
        return result;
    }
    
    /// <summary>
    /// Updates test state to <see cref="TestState.Running"/> or <see cref="TestState.Autobenched"/>.
    /// </summary>
    /// <param name="test"></param>
    public void SetRunningState(Test test)
    {
        if (test.State is TestState.Finished or TestState.Stopped) return;
        
        var state = test.AutobenchState is null || test.AutobenchState!.Resolved
            ? TestState.Running : TestState.Autobenched;

        test.State = state;
        Update(test);
        SaveChanges();
    }
    
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
    
    private IQueryable<Test> Include()
        => Context.Tests
            .Include(t => t.Engine)
            .Include(t => t.WorkerLogs)
            .Include(t => t.AutobenchState)
            .Include(t => t.OpeningBook);

    private bool AnyRunningTest(bool autobench)
        => Context.Tests
            .Any(t => autobench ? t.State == TestState.Autobenched : t.State == TestState.Running); 
    
    private Test? PausedTestWithHighestPriority(bool autobench, int workerNumberOfThreads)
        => WhereFilter(Include(), autobench,  workerNumberOfThreads)
            .OrderByDescending(t => t.Priority)
            .FirstOrDefault();

    private IQueryable<Test> WhereFilter(IQueryable<Test> tests, bool autobench, int workerNumberOfThreads)
        => tests.Where(t => t.NumberOfThreads <= workerNumberOfThreads &&
                            (!t.Autobenched ? t.Autobenched == autobench : t.AutobenchState!.Resolved == !autobench));

}