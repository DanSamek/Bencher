using Microsoft.EntityFrameworkCore;
using WebApplication.Components.Pages.Tests;
using WebApplication.Data;
using WebApplication.Data.Models;
using WebApplication.SPRT;

namespace WebApplication.Stores;

public class TestStore : Store<Test>
{
    private readonly SprtSettingsStore _sprtSettingsStore;
    private readonly OpeningBookStore _openingBookStore;
    private readonly EngineStore _engineStore;
    private readonly UserStore _userStore;
    private readonly TestBranchStore _branchStore;
    private readonly PentaStore _pentaStore;
    
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public TestStore(IDbContextFactory<ApplicationDbContext> factory, SprtSettingsStore sprtSettingsStore, OpeningBookStore openingBookStore, EngineStore engineStore, UserStore userStore, TestBranchStore branchStore, PentaStore pentaStore) : base(factory)
    {
        _sprtSettingsStore = sprtSettingsStore;
        _openingBookStore = openingBookStore;
        _engineStore = engineStore;
        _userStore = userStore;
        _branchStore = branchStore;
        _pentaStore = pentaStore;
    }
    
    /// <inheritdoc /> 
    protected override DbSet<Test> GetDbSet() => Context.Tests;
    
    /// <inheritdoc />
    /// NOTE: Loads all entities related to the test [not tracked!].
    public override Test? GetById(int id)
    {
        var result = IncludeForView()
            .AsNoTracking()
            .FirstOrDefault(t => t.Id == id);
        
        return result;
    }

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
    /// TODO update docs
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
        
        // Get maximum priority of running tests.
        var runningPriority = Context.Tests
            .Where(t => t.State == TestState.Autobenched || t.State == TestState.Running)
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
                .OrderByDescending(t => t.ThreadScale)
                .OrderByLastConnectedWorker();
        }
        else
        {
            filteredRunningTests = filteredRunningTests
                .OrderByDescending(t =>
                    (t.ThreadScale / 2) / (t.WorkerLogs.Where(wl => wl.NumberOfGames != wl.TotalNumberOfGames)
                        .Sum(wl => wl.NumberOfThreads)));
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
        =>  await GetDbSet()
            .Where(t => t.Id == testId)
            .ExecuteUpdateAsync(spc => spc
                .SetProperty(t => t.State, TestState.Stopped)
                .SetProperty(t => t.Ended, DateTime.UtcNow));

    
    /// <summary>
    /// Sets test state as finished.
    /// NOTE: Also Ended is updated to "now".
    /// </summary>
    public async Task SetFinishedState(int testId)
    {
        await GetDbSet()
            .Where(t => t.Id == testId)
            .ExecuteUpdateAsync(spc => spc
                .SetProperty(t => t.State, TestState.Finished)
                .SetProperty(t => t.Ended, DateTime.UtcNow));
    }
    
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
    
    /// <summary>
    /// Returns "count" recent tests. 
    /// </summary>
    public IReadOnlyList<Test> RecentTests(int engineId, int count)
    {
        var result = 
            GetDbSet()
                .AsNoTracking()
                .Where(t => t.Engine.Id == engineId)
                .OrderByDescending(t => t.Created)
                .Take(count)
                .ToArray();
        
        return result;
    }
    
    /// <summary>
    /// Creates a test.
    /// </summary>
    /// <param name="userId">Id of the user, that created the test.</param>
    /// <param name="data">formData</param>
    public Test Create(string userId, CreateTestFormModel data)
    {
        var sprtSettings = _sprtSettingsStore.
            GetExistingSprtSettingsOrCreate(data.Elo0!.Value, data.Elo1!.Value, data.Alpha!.Value, data.Beta!.Value);
        var openingBook = _openingBookStore.GetById(data.OpeningBookId!.Value)!;
        var engine = _engineStore.GetById(data.EngineId!.Value)!;
        var user = _userStore.GetById(userId)!;
        
        Attach(user);
        Attach(engine);
        Attach(openingBook);
        Attach(sprtSettings);
        
        // TODO load this information from the git commit message.
        var baseBranch = AddBranch(data.BaseBranchName, data.BaseBranchBench);
        var testBranch = AddBranch(data.TestBranchName, data.TestBranchBench);
        Attach(baseBranch);
        Attach(testBranch);
        
        var test = new Test
        {
            AdditionalFastchessOptions = data.AdditionalFastchessOptions,
            Description = data.Description,
            ExpectedNps = data.ExpectedNps,
            Name = data.TestName,
            Created = DateTime.UtcNow,
            Priority = data.Priority!.Value,
            NumberOfThreads = data.NumberOfThreads!.Value,
            HashSize = data.HashSize!.Value,
            TimeManagement = data.TimeManagement,
            State = TestState.Paused,
            Settings = sprtSettings,
            OpeningBook = openingBook,
            Errors = [],
            WorkerLogs = [],
            Engine = engine,
            User = user,
            Autobenched = data.Autobenched, 
            BaseBranch = baseBranch,
            BaseBranchId = baseBranch.Id,
            TestBranch = testBranch,
            TestBranchId = testBranch.Id
        };
        test.CalculateThreadScale();
        
        test = GetDbSet().Add(test).Entity;
        SaveChanges();
        
        sprtSettings.Tests.Add(test);
        baseBranch.BaseBranchOf = test;
        testBranch.TestBranchOf = test;
        var penta = _pentaStore.AddRet(new Penta
        {
            Test = test
        });
        test.Penta = penta;
        
        SaveChanges();
        
        if (!data.Autobenched) return test;
        
        var autobenchState = new AutobenchState
        {
            Test = test,
            TestId = test.Id,
            UserConfidence = data.Confidence!.Value
        };
        
        autobenchState = Context.AutobenchStates.Add(autobenchState).Entity;
        test.AutobenchState = autobenchState;
        SaveChanges();
        return test;

        TestBranch AddBranch(string name, int bench)
        {
            return _branchStore.AddRet(new TestBranch
            {
                Bench = bench,
                Name = name,
                Engine = engine
            });
        }
    }

    /// <summary>
    /// Result from <see cref="TestStore.GetRunningTests" /> 
    /// </summary> 
    public record RunningTestResult(IReadOnlyList<Test> AutobenchedTests, IReadOnlyList<Test> RunningTests);
    
    /// <summary>
    /// Returns all running tests (autobenched, normal ones)
    /// </summary>
    public RunningTestResult GetRunningTests()
    {
        var runningTests = GetByState(TestState.Running);
        var autobenchedTests = GetByState(TestState.Autobenched);
        return new RunningTestResult(autobenchedTests, runningTests);
        
        IReadOnlyList<Test> GetByState(TestState state) 
            => IncludeForView()
                .Where(t => t.State == state)
                .ToArray();
    }
    
    /// <summary>
    /// Returns all paused tests.
    /// </summary>
    public IReadOnlyList<Test> GetPausedTests()
    {
        var result = IncludeForView()
            .Where(t => t.State == TestState.Paused)
            .ToArray();
        
        return result;
    }

    /// <summary>
    /// Sets test to PausedState if there are no active workers for a test.
    /// </summary>
    /// <returns>If the test is in the running state</returns>
    public async Task<bool> SetPausedIfNoActiveWorkers(int testId)
    {
        var stillRunning = Context.WorkerLogs
            .Any(wl => wl.Test.Id == testId && wl.State == WorkerLogState.Active);
        
        if (!stillRunning)
        {
            await GetDbSet()
                .Where(t => t.Id == testId)
                .ExecuteUpdateAsync(spc => spc.SetProperty(t => t.State, TestState.Paused));
        }

        return stillRunning;
    }
    
    /// <summary>
    /// Returns ended tests for a page.
    /// So the test state is stopped or finished.
    /// Ordered by time - the last will be the first.
    /// </summary>
    public IReadOnlyList<Test> GetEndedTestsForPage(int pageIndex, int pageSize = WebConstants.PAGE_SIZE)
     => IncludeForView() 
         .Where(t => t.State == TestState.Finished || t.State == TestState.Stopped)
         .OrderByDescending(t => t.Ended)
         .TakePage(pageIndex, pageSize)
         .ToArray();
    
    
    /// <summary>
    /// Returns all passed tests for a page.
    /// Ordered by time - the last will be the first.
    /// </summary>
    public IReadOnlyList<Test> GetPassedTestsForPage(int pageIndex, int pageSize = WebConstants.PAGE_SIZE)
    {
        var finishedTests = IncludeForView()
            .Where(t => t.State == TestState.Finished)
            .OrderByDescending(t => t.Ended);
        
        var result = new List<Test>();
        foreach (var finishedTest in finishedTests)
        {
            var sprtStatistics = Sprt.GetStatistics(finishedTest);
            if (sprtStatistics.Result == Sprt.SprtResult.H1Accepted)
            {
                result.Add(finishedTest);
            }
        }
        
        return result
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToArray();
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
            .Where(t => t.State == TestState.Paused)
            .OrderByDescending(t => t.Priority)
            .OrderByLastConnectedWorker()
            .FirstOrDefault();

    private IQueryable<Test> WhereFilter(IQueryable<Test> tests, bool autobench, int workerNumberOfThreads)
    {
        return tests
            .Where(t => (autobench || t.NumberOfThreads <= workerNumberOfThreads) &&
                        (!t.Autobenched
                            ? t.Autobenched == autobench
                            : t.AutobenchState!.Resolved ==
                              !autobench)); 
    }
    
    private IQueryable<Test> IncludeForView()
        =>
            GetDbSet()
                .Include(t => t.Engine)
                .Include(t => t.User)
                .Include(t => t.Penta)
                .Include(t => t.Settings)
                .Include(t => t.BaseBranch)
                .Include(t => t.TestBranch)
                .Include(t => t.WorkerLogs)
                .Include(t => t.OpeningBook)
                .Include(t => t.AutobenchState);

    
}

file static class LinqExtensions
{
    public static IOrderedQueryable<Test> OrderByLastConnectedWorker(this IOrderedQueryable<Test> tests)
    {
        return tests.ThenBy(t =>
            t.WorkerLogs
                .Where(wl => wl.Test.Id == t.Id)
                .Select(wl => wl.LastConnectTime)
                .Max()
            ?? DateTime.MinValue);   
    }

}