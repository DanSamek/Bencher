using Microsoft.EntityFrameworkCore;
using WebApplication.Data;

namespace WebApplication.Tests;

public class TestContextFactory : IDbContextFactory<ApplicationDbContext>
{
    private string _connectionString = null!;
    
    private DbContextOptions<ApplicationDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

    public ApplicationDbContext CreateDbContext()
    {
        var context = new ApplicationDbContext(CreateOptions());
        context.Database.EnsureCreated(); // optionally call only once externally
        return context;
    }
    
    public void SetConnectionString(string connectionString) => _connectionString = connectionString;
}