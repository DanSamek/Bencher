using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Dtos.Requests;
using Shared.Dtos.Responses;
using WebApplication.API;
using WebApplication.Data.Models;
using WebApplication.Stores;
using WebApplication.Tests.Builders;

namespace WebApplication.Tests.API;

[TestFixture]
public class WorkerLogWatcherTests : WorkerControllerTestBase
{
    private WorkerLogWatcher _watcher;

    [TearDown]
    public void TearDown()
    {
        _watcher.Dispose();
    }
    
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        var scopeFactory = new TestScopeFactory(Factory, CreateTestStore());
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
    /// Watcher test with all running tests + all of them has DateTime.UtcNow as LastConnectTime.
    /// We expect, that all tests will be still in the running state.
    /// </summary>
    [Test]
    public async Task DateTimeNow_AsLastConnectTime()
    {
        CreateWorkerLogs(5);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(async () => { await _watcher.StartAsync(new CancellationToken()); });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        // Not the best solution :D
        await Task.Delay(TimeSpan.FromSeconds(1));

        await using var context = Factory.CreateDbContext();
        var activeWorkerLogs = context.WorkerLogs
            .Count(x => x.State == WorkerLogState.Active);
        
        Assert.That(activeWorkerLogs, Is.EqualTo(5));
    }
    
    /// <summary>
    /// Watcher test with all running tests + all of them has DateTime.UtcNow - (1 min 1 sec) as the LastConnectTime.
    /// We expect, that all tests will be still in the disconnected state.
    /// </summary>
    [Test]
    public async Task DateTimeNow_OutOfRange()
    {
        CreateWorkerLogs(5);
        await using var changingContext = Factory.CreateDbContext();
        var toChangeDateTime = DateTime.UtcNow.Subtract(new TimeSpan(0,1, 1));
        await changingContext.WorkerLogs.ExecuteUpdateAsync(spc => spc.SetProperty(wl => wl.LastConnectTime, toChangeDateTime));
        
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(async () => { await _watcher.StartAsync(new CancellationToken()); });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
    
    /// <summary>
    /// When watcher sets workerlogs to the disconnected state,
    /// it can happen that test has no workers -> test should be paused.
    /// </summary>
    [Test]
    public async Task StopTest()
    {
        CreateWorkerLogs(5);

        var context = Factory.CreateDbContext();
        var tests = context.Tests.ToArray();
        Assert.That(tests, Has.Length.EqualTo(1));
        Assert.That(tests[0].State, Is.EqualTo(TestState.Running));

        var workerLogs = context.WorkerLogs
            .AsNoTracking()
            .ToArray();
        foreach (var workerLog in workerLogs)
        {
            Assert.That(workerLog.State, Is.EqualTo(WorkerLogState.Active));
        }
        
        await context.WorkerLogs
            .ExecuteUpdateAsync(spc => spc.SetProperty(we => we.LastConnectTime, DateTime.MinValue.ToUniversalTime()));
        
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(async () => { await _watcher.StartAsync(new CancellationToken()); });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        // Not the best solution :D
        await Task.Delay(TimeSpan.FromSeconds(1));

        context = Factory.CreateDbContext();
        tests = context.Tests.ToArray();
        Assert.That(tests, Has.Length.EqualTo(1));
        Assert.That(tests[0].State, Is.EqualTo(TestState.Paused));
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

file class TestServiceProvider(TestContextFactory factory, TestStore testStore) : IServiceProvider
{
    public object GetService(Type serviceType)
    {
        if (serviceType == typeof(TestStore))
        {
            return testStore;
        }
        return factory.CreateDbContext();
    }
}

file class TestServiceScope(TestContextFactory factory, TestStore testStore) : IServiceScope
{
    public IServiceProvider ServiceProvider { get; } = new TestServiceProvider(factory, testStore);
    public void Dispose() {}
}

file class TestScopeFactory(TestContextFactory factory, TestStore testStore) : IServiceScopeFactory
{
    public IServiceScope CreateScope()
    {
        var serviceScope = new TestServiceScope(factory, testStore);
        return serviceScope;
    }
}