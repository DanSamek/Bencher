using Microsoft.EntityFrameworkCore;
using WebApplication.Data;

namespace WebApplication.Tests;

public class TestContextFactory : IDbContextFactory<ApplicationDbContext>
{
    private readonly string _guid;
 
    /// <summary>
    /// .Ctor
    /// </summary>
    public TestContextFactory() => _guid = Guid.NewGuid().ToString();
    
    /// <inheritdoc /> 
    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"TEST_DATABASE_{_guid}") // TODO use different InMemoryDb, microsoft in-memory doesn't have implemented .ExecuteUpdate/Delete
            .Options;

        var instance = new ApplicationDbContext(options);
        return instance;
    }
}