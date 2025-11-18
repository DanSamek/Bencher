using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class ErrorStore : Store<Error>
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public ErrorStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory){}

    /// <inheritdoc /> 
    protected override DbSet<Error> GetDbSet() => Context.Errors;

    /// <summary>
    /// Returns all errors ordered by time - last will be first.
    /// </summary>
    public IReadOnlyList<Error> GetErrors()
        => GetDbSet()
            .Include(e => e.Test)
            .Include(e => e.WorkerLog)
            .OrderByDescending(t => t.Time)
            .ToArray();
}