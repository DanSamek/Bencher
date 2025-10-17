using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

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
    
    private void ClearDb()
    {
        var context = Factory.CreateDbContext();
        context.Users.ExecuteDelete();
        context.Engines.ExecuteDelete();
        context.TestBranches.ExecuteDelete();
        context.Tests.ExecuteDelete();
        context.OpeningBooks.ExecuteDelete();
        context.SprtSettings.ExecuteDelete();
        context.Pentas.ExecuteDelete();
        context.Errors.ExecuteDelete();
        context.WorkerLogs.ExecuteDelete();
        context.AutobenchStates.ExecuteDelete();
    }
    
    protected TestContextFactory CreateContextFactory()
    {
        var factory = new TestContextFactory();
        factory.SetConnectionString(_container.GetConnectionString());
        return factory;
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