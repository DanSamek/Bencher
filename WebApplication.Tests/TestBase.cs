using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using WebApplication.Data.Models;
using WebApplication.Stores;

namespace WebApplication.Tests;

public class TestBase
{
    protected TestContextFactory Factory { get; private set; } = null!;
    
    private PostgreSqlContainer _container = null!;
    
    [OneTimeSetUp]
    public async Task OneTimeSetUp() => _container = await CreateContainer();
    
    [SetUp]
    public virtual void Setup()
    {
        Factory = CreateContextFactory();
        ClearDb();
    }

    protected TestContextFactory CreateContextFactory()
    {
        var factory = new TestContextFactory();
        factory.SetConnectionString(_container.GetConnectionString());
        return factory;
    }
    
    
    /// <summary>
    /// Creates TestStore instance. 
    /// </summary>
    protected TestStore CreateTestStore()
    {
        var testStore = new TestStore(Factory, new SprtSettingsStore(Factory), new OpeningBookStore(Factory),
            new EngineStore(Factory), new UserStore(Factory), new TestBranchStore(Factory), new PentaStore(Factory));

        return testStore;
    }
    
    private void ClearDb()
    {
        var context = Factory.CreateDbContext();
        context.OpeningBooks.ExecuteDelete();
        context.Users.ExecuteDelete();
        context.Engines.ExecuteDelete();
        context.TestBranches.ExecuteDelete();
        context.Tests.ExecuteDelete();
        context.SprtSettings.ExecuteDelete();
        context.Pentas.ExecuteDelete();
        context.Errors.ExecuteDelete();
        context.WorkerLogs.ExecuteDelete();
        context.AutobenchStates.ExecuteDelete();
    }

    private static async Task<PostgreSqlContainer> CreateContainer()
    {
        var container = new PostgreSqlBuilder()
            .WithImage("postgres:15.1")
            .Build();
        await container.StartAsync();
        return container;
    }
}