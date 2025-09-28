using Microsoft.EntityFrameworkCore;
using WebApplication.Data;

namespace WebApplication.Tests;

public class TestContextFactory : IDbContextFactory<ApplicationDbContext>
{
    /// <inheritdoc /> 
    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TEST_DATABASE")
            .Options;

        var instance = new ApplicationDbContext(options);
        return instance;
    }
}