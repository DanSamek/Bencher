using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Tests.Builders;


public class EngineBuilder
{
    private readonly Engine _engine;
    private readonly ApplicationDbContext _context;
    private readonly UserBuilder _userBuilder;
    private readonly ApplicationUser _user;
    public EngineBuilder(Engine engine, ApplicationDbContext context, UserBuilder userBuilder, ApplicationUser user)
    {
        _engine = engine;
        _context = context;
        _userBuilder = userBuilder;
        _user = user;
    }
    
    public UserBuilder Close() => _userBuilder;

    public EngineBuilder AddBranch(string name, int bench = 25065652)
    {
        var branch = new TestBranch
        {
            Bench = bench,
            Name = name,
            Engine = _engine
        };

        _context.TestBranches.Add(branch);
        _context.SaveChanges();
        
        return this;
    }
    
    // KISS
    public TestBuilder AddAutobenchedTest(string name, 
        string bookName,
        string baseBranchName,
        string testBranchName,
        string timeManagement = "8+0.08", 
        int priority = 0, 
        int numberOfThreads = 1,
        TestState state = TestState.Paused,
        int bench = 1000000,
        double userConfidence = 0.1)
    {
        var baseBranch = _context.TestBranches.First(x => x.Name == baseBranchName);
        var testBranch = _context.TestBranches.First(x => x.Name == testBranchName);
        var test = new Test
        {
            Name = name,
            Created = DateTime.UtcNow,
            Priority = priority,
            NumberOfThreads = numberOfThreads,
            HashSize = 16,
            TimeManagement = timeManagement,
            State = state,
            Settings = _context.SprtSettings.First(), 
            OpeningBook = _context.OpeningBooks.First(x => x.Name == bookName),
            Errors = [],
            WorkerLogs = [],
            Engine = _engine,
            BaseBranch = baseBranch,
            BaseBranchId = baseBranch.Id,
            TestBranch = testBranch,
            TestBranchId = testBranch.Id,
            User = _user,
            Autobenched = true
        };
        test = UpdateTest(_context, test, _user);
        
        var autobenchState = new AutobenchState
        {
            Bench = bench,
            UserConfidence = userConfidence,
            Test = test,
            TestId = test.Id,
        };
        
        _context.AutobenchStates.Add(autobenchState);
        _context.SaveChanges();
        
        var testBuilder = new TestBuilder(this, test, _user, _context);
        return testBuilder;
    }
    
    // KISS
    public TestBuilder AddTest(string name, 
                                 string bookName,
                                 string baseBranchName,
                                 string testBranchName,
                                 string timeManagement = "8+0.08", 
                                 int priority = 0, 
                                 int numberOfThreads = 1,
                                 TestState state = TestState.Paused)
    {
        var baseBranch = _context.TestBranches.First(x => x.Name == baseBranchName);
        var testBranch = _context.TestBranches.First(x => x.Name == testBranchName);
        var test = new Test
        {
            Name = name,
            Created = DateTime.UtcNow,
            Priority = priority,
            NumberOfThreads = numberOfThreads,
            HashSize = 16,
            TimeManagement = timeManagement,
            State = state,
            Settings = _context.SprtSettings.First(), 
            OpeningBook = _context.OpeningBooks.First(x => x.Name == bookName),
            Errors = [],
            WorkerLogs = [],
            Engine = _engine,
            BaseBranch = baseBranch,
            BaseBranchId = baseBranch.Id,
            TestBranch = testBranch,
            TestBranchId = testBranch.Id,
            User = _user,
            Autobenched = false
        };
       
        
        test = UpdateTest(_context, test, _user);
        var testBuilder = new TestBuilder(this, test, _user, _context);
        return testBuilder;
    }
    
    public static void AddAutobenchedTestForUser(
        string name,
        string bookName,
        string baseBranchName,
        string testBranchName,
        string engineName,
        string username,  
        ApplicationDbContext context,
        string timeManagement = "8+0.08", 
        int priority = 0, 
        int numberOfThreads = 1,
        TestState state = TestState.Paused,
        int bench = 1000000,
        double userConfidence = 0.1,
        params int[] workerThreads)
    {
        var engine = context.Engines.First(e => e.Name == engineName);
        var user = context.Users.First(u => u.UserName == username);

        var baseBranch = context.TestBranches.First(x => x.Name == baseBranchName);
        var testBranch = context.TestBranches.First(x => x.Name == testBranchName);
        var test = new Test
        {
            Name = name,
            Created = DateTime.UtcNow,
            Priority = priority,
            NumberOfThreads = numberOfThreads,
            HashSize = 16,
            TimeManagement = timeManagement,
            State = state,
            Settings = context.SprtSettings.First(),
            OpeningBook = context.OpeningBooks.First(x => x.Name == bookName),
            Errors = [],
            WorkerLogs = [],
            Engine = engine,
            BaseBranch = baseBranch,
            BaseBranchId = baseBranch.Id,
            TestBranch = testBranch,
            TestBranchId = testBranch.Id,
            User = user,
            Autobenched = true
        };
        test = UpdateTest(context, test, user);
        
        var penta = context.Pentas.Add(new Penta
        {
            Test = test,
            TestId = test.Id,
        }).Entity;
        
        context.SaveChanges();
        
        test.Penta = penta;
        context.SaveChanges();
        
        var autobenchState = new AutobenchState
        {
            Bench = bench,
            UserConfidence = userConfidence,
            Test = test,
            TestId = test.Id,
        };
        
        context.AutobenchStates.Add(autobenchState);
        context.SaveChanges();


        foreach (var numberOfWorkerThreads in workerThreads)
        {
            var wl = new WorkerLog
            {
                Mac = "AA:BB:CC:DD:EE:FF",
                NumberOfGames = 0,
                TotalNumberOfGames = 64,
                LastConnectTime = DateTime.UtcNow,
                NumberOfThreads = numberOfWorkerThreads,
                ConnectTime = DateTime.UtcNow,
                User = user,
                Test = test,
                State = WorkerLogState.Active,
                InitialTestState = InitialTestState.Autobenched
            };
            
            context.WorkerLogs.Add(wl);
            test.WorkerLogs.Add(wl);
            context.SaveChanges();
        }
        
        Console.WriteLine(context.WorkerLogs.Count());
        
    }
    
    
    private static Test UpdateTest(ApplicationDbContext context, Test test, ApplicationUser user)
    {
        test.CalculateThreadScale();
        test = context.Tests.Add(test).Entity;
        context.SaveChanges();
        
        user.Tests.Add(test);
        context.SaveChanges();
        return test;
    }
}