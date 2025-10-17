using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApplication.API;
using WebApplication.API.Dtos.Requests;
using WebApplication.API.Dtos.Responses;
using WebApplication.Data.Models;
using WebApplication.Tests.Builders;

namespace WebApplication.Tests.API;

[TestFixture]
public class WorkerLogWatcherTests : WorkerControllerTestBase
{
    private WorkerLogWatcher _watcher;
    
    [SetUp]
    public override void Setup()
    {
        base.Setup();    
        var scopeFactory = new TestScopeFactory(Factory);
        _watcher = new WorkerLogWatcher(scopeFactory);
        RefreshController();

        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("uho")
            .CreateSprtSettings()
            .CreateUser("user_1")
            .WithAccessToken("12345689")
            .Close()
            .CreateUser("user_2")
            .WithAccessToken("87654321")
            .AddEngine("sentinel")
            .AddBranch("base_branch")
            .AddBranch("test_branch")
            .AddTest("test_11", "uho", "base_branch", "test_branch")
            .EnsurePentaCreated(Factory.CreateDbContext())
            .Close()
            .Close()
            .Close();
    }

    /// <summary>
    /// Watcher test with all running tests + all of them has DateTime.Now as LastConnectTime.
    /// We expect, that all tests will be still in the running state.
    /// </summary>
    [Test]
    public async Task DateTimeNow_AsLastConnectTime()
    {
        CreateWorkerLogs(5);
        Task.Run(async () => { await _watcher.StartAsync(new CancellationToken()); });
        // Not the best solution :D
        await Task.Delay(TimeSpan.FromSeconds(1));

        await using var context = Factory.CreateDbContext();
        var activeWorkerLogs = context.WorkerLogs
            .Count(x => x.State == WorkerLogState.Active);
        
        Assert.That(activeWorkerLogs, Is.EqualTo(5));
    }
    
    /// <summary>
    /// Watcher test with all running tests + all of them has DateTime.Now - (1 min 1 sec) as the LastConnectTime.
    /// We expect, that all tests will be still in the disconnected state.
    /// </summary>
    [Test]
    public async Task DateTimeNow_OutOfRange()
    {
        CreateWorkerLogs(5);
        await using var changingContext = Factory.CreateDbContext();
        var toChangeDateTime = DateTime.Now.Subtract(new TimeSpan(0,1, 1));
        await changingContext.WorkerLogs.ExecuteUpdateAsync(spc => spc.SetProperty(wl => wl.LastConnectTime, toChangeDateTime));
        
        Task.Run(async () => { await _watcher.StartAsync(new CancellationToken()); });
        // Not the best solution :D
        await Task.Delay(TimeSpan.FromSeconds(1));

        await using var context = Factory.CreateDbContext();
        var activeWorkerLogs = context.WorkerLogs
            .Count(x => x.State == WorkerLogState.Active);
        
        Assert.That(activeWorkerLogs, Is.EqualTo(0));
        var disconnectedWorkerLogs = context.WorkerLogs
            .Count(x => x.State == WorkerLogState.Disconnected);
        
        Assert.That(disconnectedWorkerLogs, Is.EqualTo(5));
    }
    
    
    private void CreateWorkerLogs(int count)
    {
        for (var i = 0; i < count; i++)
        {
            RefreshController();
            LoginAs("user_1");
            var result = Controller.GetTest(new GetTestDto
            {
                Autobench = false,
                Mac = "12:34:12:34:12:34",
                Name = "TEST_WORKER",
                NumberOfThreads = 1
            });
            var resultDto = GetResponseValue<GetTestNonAutobenchResponse, OkObjectResult>(result)!;
            var connectionId = resultDto.ConnectionId;

            RefreshController();
            Controller.RunningTest(new RunningTestDto
            {
                ConnectionId = connectionId
            });
        }
    }
}


// NOTE: This can be mocked somehow, TODO.
file class TestServiceProvider(TestContextFactory factory) : IServiceProvider
{
    public object? GetService(Type serviceType) => factory.CreateDbContext();
}

file class TestServiceScope(TestContextFactory factory) : IServiceScope
{
    public IServiceProvider ServiceProvider { get; } = new TestServiceProvider(factory);
    public void Dispose() {}
}

file class TestScopeFactory(TestContextFactory factory) : IServiceScopeFactory
{
    public IServiceScope CreateScope()
    {
        var serviceScope = new TestServiceScope(factory);
        return serviceScope;
    }
}