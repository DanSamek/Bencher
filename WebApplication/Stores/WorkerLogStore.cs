using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class WorkerLogStore : StoreBase
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public WorkerLogStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory) { }

    /// <summary>
    /// Gets <see cref="WorkerLog" /> by connectionId.
    /// Note: loads with the <see cref="Test" /> entity
    /// </summary>
    public WorkerLog? GetByConnectionId(int connectionId)
    {
        var workerLog = Context.WorkerLogs
            .Include(wl => wl.Test)
            .FirstOrDefault(wl => wl.Id == connectionId);
        
        return workerLog;
    }
    /// <summary>
    /// Saves a worker log.
    /// </summary>
    public void Save(WorkerLog workerLog) => Context.WorkerLogs.Update(workerLog);
}