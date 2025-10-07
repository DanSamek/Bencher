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
    
    public EngineBuilder AddTest(string name, 
                                 string bookName,
                                 string baseBranchName,
                                 string testBranchName,
                                 string timeManagement = "8+0.08", 
                                 int priority = 0, 
                                 int numberOfThreads = 1)
    {
        var penta = new Penta();
        var test = new Test
        {
            Name = name,
            Created = DateTime.Now,
            Priority = priority,
            NumberOfThreads = numberOfThreads,
            HashSize = 16,
            TimeManagement = timeManagement,
            State = TestState.Paused,
            Settings = _context.SprtSettings.First(), // TODO maybe as a parameter.
            OpeningBook = _context.OpeningBooks.First(x => x.Name == bookName),
            Errors = [],
            WorkerLogs = [],
            Engine = _engine,
            Penta = penta,
            BaseBranch = _context.TestBranches.First(x => x.Name == baseBranchName),
            TestBranch = _context.TestBranches.First(x => x.Name == testBranchName),
            User = _user,
            Autobenched = false
        };
        test.CalculateThreadScale();
        _context.Tests.Add(test);
        _context.SaveChanges();
        
        return this;
    }
}