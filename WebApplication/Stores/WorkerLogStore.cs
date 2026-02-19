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
        var error = new TestError
        {
            Time = DateTime.UtcNow,
            Test = workerLog.Test,
            WorkerLog = workerLog,
            WorkerLogId = workerLog.Test.Id
        };
        
        var entity = Context.TestErrors.Add(error).Entity;
        workerLog.Error = entity;
        Context.WorkerLogs.Update(workerLog);
        
        var test = Context.Tests.First(t => t.Id == workerLog.Test.Id);
        test.Errors.Add(entity);
        Context.SaveChanges();
        
        var logContent = new ErrorContent
        {
            Data = errorData,
            ErrorId = entity.Id,
        };
        entity.Log = logContent;
        Context.SaveChanges();
    }

    /// <summary>
    /// Stops all workers (state is set to Finished).
    /// </summary>
    public async Task StopAllWorkers(int testId)
    {
        await GetDbSet()
            .Where(wl => wl.Test.Id == testId && wl.State == WorkerLogState.Active)
            .ExecuteUpdateAsync(spc => spc.SetProperty(wl => wl.State, WorkerLogState.Finished));
    }
    
    /// <summary>
    /// Sets finished state for all autobenched workers. 
    /// </summary>
    public async Task SetActiveAutobenchWorkersAsFinished(int testId)
    {
        await GetDbSet()
            .Where(wl => wl.Test.Id == testId && wl.TotalNumberOfGames == 0 /*Autobenched*/ && wl.State == WorkerLogState.Active)
            .ExecuteUpdateAsync(spc => spc.SetProperty(wl => wl.State, WorkerLogState.Finished));
    }
}