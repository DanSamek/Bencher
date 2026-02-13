using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class WorkerErrorStore : Store<WorkerError>
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public WorkerErrorStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory) {}

    /// <inheritdoc /> 
    protected override DbSet<WorkerError> GetDbSet() => Context.WorkerErrors;
    
    /// <summary>
    /// Adds worker error.
    /// </summary>
    public void AddError(byte[] errorData)
    {
        var entity = new WorkerError
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
    
    /// <summary>
    /// Loads worker error content.
    /// </summary>
    public byte[] LoadContent(int errorId)
        => GetDbSet()
            .Include(e => e.Log)
            .Where(e => e.Id == errorId)
            .Select(e => e.Log.Data)
            .First();
    
    /// <summary>
    /// Returns errors for the paging ordered by time - the last will be the first.
    /// ! Without log.
    /// </summary>
    public IReadOnlyList<WorkerError> GetErrorsForPage(int pageIndex, int pageSize = WebConstants.PAGE_SIZE)
        => GetDbSet()
            .OrderByDescending(t => t.Time)
            .TakePage(pageIndex, pageSize)
            .ToArray();
    
}