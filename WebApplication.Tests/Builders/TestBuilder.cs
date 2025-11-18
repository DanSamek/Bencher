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
            LastConnectTime = DateTime.UtcNow,
            NumberOfThreads = numberOfWorkerThreads,
            ConnectTime = DateTime.UtcNow,
            User = _user,
            Test = _test,
            State = WorkerLogState.Active,
            InitialTestState = _test.Autobenched ? InitialTestState.Autobenched : InitialTestState.Normal
        };
        
        _context.WorkerLogs.Add(wl);
        _test.WorkerLogs.Add(wl);
        _context.SaveChanges();
        return this;
    }

    public TestBuilder EnsurePentaCreated(ApplicationDbContext context)
    {
        context.Attach(_test);
        var penta = new Penta
        {
            Test = _test
        };
        context.Pentas.Add(penta);
        context.SaveChanges();
        return this;
    }

    public TestBuilder AddError(ApplicationDbContext context, DateTime date)
    {
        context.Attach(_test);
        var wl = context.WorkerLogs.First(wl => wl.Test.Id == _test.Id);
        var error = new Error
        {
            Time = date.ToUniversalTime(),
            Test = _test,
            WorkerLog = wl,
            
        };
        error = context.Errors.Add(error).Entity;
        context.SaveChanges();

        error.Log = new ErrorContent
        {
            Data = [0x1, 0x2, 0x4],
            ErrorId = error.Id
        };
        return this;
    }
    
    public EngineBuilder Close() => _engineBuilder;

}