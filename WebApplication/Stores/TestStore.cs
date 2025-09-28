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
    /// Note, this method also includes <see cref="Test.WorkerLogs" /> and <see cref="Test.Engine" />
    /// </summary>
    public Test? GetNextTestForWorker(bool autobench)
    {
        var runningTest = Context.Tests
            .Include(t => t.Engine)
            .Include(t => t.WorkerLogs)
            .Where(t => t.State == TestState.Running && t.Autobenched == autobench)
            .OrderByDescending(t => t.ThreadScale)
            .ToArray();
        
        var result = runningTest.FirstOrDefault(t => t.WorkerLogs.Count == 0) 
                          ?? runningTest.MaxBy(t => (t.ThreadScale / 2) / t.ActiveWorkerThreadCount());
        
        return result;
    }
    
    /// <summary>
    /// Updates test entity
    /// </summary>
    public void Update(Test test) => Context.Tests.Update(test);

    /// <summary>
    /// Stops a test by a id.
    /// </summary>
    public async Task StopTest(int testId)
        => await Context.Tests
            .Where(t => t.Id == testId)
            .ExecuteUpdateAsync(spc => spc.SetProperty(t => t.State, TestState.Stopped));
}