using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class WorkerLogStore : Store<WorkerLog>
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public WorkerLogStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory) { }
    
    /// <inheritdoc /> 
    protected override DbSet<WorkerLog> GetDbSet() =>  Context.WorkerLogs;
    
    /// <summary>
    /// Gets <see cref="WorkerLog" /> by connectionId.
    /// Note: loads with the <see cref="Test" /> entity and AutobenchState.
    /// Note: it's tracked.
    /// </summary>
    public WorkerLog? GetByConnectionId(int connectionId)
    {
        var workerLog = Context.WorkerLogs
            .Include(wl => wl.Test)
                .ThenInclude(t => t.AutobenchState)
            .FirstOrDefault(wl => wl.Id == connectionId);
        
        return workerLog;
    }
    
    /// <summary>
    /// Adds error to the worker log.
    /// NOTE: WorkerLog has to be tracked.
    /// </summary>
    public void AddError(WorkerLog workerLog, byte[] errorData)
    {
        var error = new Error
        {
            Time = DateTime.UtcNow,
            Test = workerLog.Test,
            WorkerLog = workerLog
        };
        
        var entity = Context.Errors.Add(error).Entity;
        workerLog.Errors.Add(entity);
        Context.WorkerLogs.Update(workerLog);
        Context.SaveChanges();
        
        var logContent = new ErrorContent
        {
            Data = errorData,
            ErrorId = entity.Id,
        };
        entity.Log = logContent;
        Context.SaveChanges();
    }
}