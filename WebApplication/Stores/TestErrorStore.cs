using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class TestErrorStore : Store<TestError>
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public TestErrorStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory){}

    /// <inheritdoc /> 
    protected override DbSet<TestError> GetDbSet() => Context.TestErrors;

    /// <summary>
    /// Returns all errors ordered by time - last will be first.
    /// ! Without data.
    /// </summary>
    public IReadOnlyList<TestError> GetErrors()
        => GetDbSet()
            .Include(e => e.Test)
            .Include(e => e.WorkerLog)
            .OrderByDescending(t => t.Time)
            .ToArray();
}