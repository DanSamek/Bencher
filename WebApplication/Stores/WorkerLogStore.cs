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
    /// Note: loads with the <see cref="Test" /> entity and AutobenchState
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
    public void AddError(WorkerLog workerLog, Error error)
    {
        workerLog.Errors.Add(error);
        Context.WorkerLogs.Update(workerLog);
        Context.SaveChanges();
    }
}