using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebApplication.Data;

namespace WebApplication.Tests;

public class TestContextFactory : IDbContextFactory<ApplicationDbContext>
{
    private DbConnection? _connection;
    
    private DbContextOptions<ApplicationDbContext> CreateOptions() => 
        new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection!).Options;
    
    /// <inheritdoc /> 
    public ApplicationDbContext CreateDbContext()
    {
        if (_connection is not null) return new ApplicationDbContext(CreateOptions());
        
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
            
        using var context = new ApplicationDbContext(CreateOptions());
        context.Database.EnsureCreated();

        return new ApplicationDbContext(CreateOptions());
    }
}