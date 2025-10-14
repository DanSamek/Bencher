using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Tests.Builders;

public class TestBuilder
{
    private readonly EngineBuilder _engineBuilder;
    private readonly Test _test;
    private readonly ApplicationUser _user;
    private readonly ApplicationDbContext _context;
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public TestBuilder(EngineBuilder userBuilder, Test test, ApplicationUser user, ApplicationDbContext context)
    {
        _engineBuilder = userBuilder;
        _test = test;
        _user = user;
        _context = context;
    }
    
    public TestBuilder AddWorker(int numberOfWorkerThreads)
    {
        var wl = new WorkerLog
        {
            Mac = "AA:BB:CC:DD:EE:FF",
            NumberOfGames = 0,
            TotalNumberOfGames = 64,
            LastConnectTime = DateTime.Now,
            NumberOfThreads = numberOfWorkerThreads,
            ConnectTime = DateTime.Now,
            User = _user,
            Test = _test
        };
        
        _context.WorkerLogs.Add(wl);
        _test.WorkerLogs.Add(wl);
        _context.SaveChanges();
        return this;
    }

    public EngineBuilder Close() => _engineBuilder;
}