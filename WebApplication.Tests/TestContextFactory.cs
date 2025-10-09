using Microsoft.EntityFrameworkCore;
using WebApplication.Data;

namespace WebApplication.Tests;

public class TestContextFactory : IDbContextFactory<ApplicationDbContext>
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    /// <summary>
    /// .Ctor
    /// </summary>
    public TestContextFactory()
    {
        var guid = Guid.NewGuid().ToString();
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"TEST_DATABASE_{guid}") // TODO use different InMemoryDb, microsoft in-memory doesn't have implemented .ExecuteUpdate/Delete
            .Options;
    }

    /// <inheritdoc /> 
    public ApplicationDbContext CreateDbContext()
    {
        var instance = new ApplicationDbContext(_options);
        return instance;
    }
}