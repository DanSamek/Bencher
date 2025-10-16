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
    /// Note: loads with the <see cref="Test" /> entity
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
    /// Saves a worker log.
    /// </summary>
    public void Save(WorkerLog workerLog) => Context.WorkerLogs.Update(workerLog);
    
    
    /// <summary>
    /// Creates a worker log.
    /// </summary>
    /// <param name="workerLog"></param>
    public void Create(WorkerLog workerLog) => Context.WorkerLogs.Add(workerLog);

    /// <summary>
    /// Adds error to the worker log.
    /// </summary>
    public void AddError(WorkerLog workerLog, Error error)
    {
        workerLog.Errors.Add(error);
        Context.WorkerLogs.Update(workerLog);
        Context.SaveChanges();
    }
}