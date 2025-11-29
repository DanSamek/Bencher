using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class WorkerErrorStore : Store<Error>
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public WorkerErrorStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory) {}

    /// <inheritdoc /> 
    protected override DbSet<Error> GetDbSet() => Context.WorkerErrors;
    
    /// <summary>
    /// Adds worker error.
    /// </summary>
    public void AddError(byte[] errorData)
    {
        var entity = new Error
        {
            Time = DateTime.UtcNow
        };
        entity = GetDbSet().Add(entity).Entity;
        var logContent = new ErrorContent
        {
            Data = errorData,
            ErrorId = entity.Id,
        };
        entity.Log = logContent;
        Context.SaveChanges();
    }
}