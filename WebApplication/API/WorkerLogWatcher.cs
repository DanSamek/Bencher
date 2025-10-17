using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.API;

/// <summary>
/// Service, that will watch all running worker logs and sets to state Disconnected,
/// if last now - connection time of the worker log is higher than  LAST_CONNECT_TIME_MIN_MAX.
/// TODO somehow test.
/// </summary>
public class WorkerLogWatcher : IHostedService
{
    private const int LAST_CONNECT_TIME_MIN_MAX = 1;
    private readonly PeriodicTimer _periodicTimer = new PeriodicTimer(new TimeSpan(0, 0, LAST_CONNECT_TIME_MIN_MAX, 0));
    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    /// <summary>
    /// .Ctor 
    /// </summary>
    public WorkerLogWatcher(IServiceScopeFactory serviceScopeFactory) => _serviceScopeFactory = serviceScopeFactory;
    
    /// <inheritdoc /> 
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var lowerBoundTime = DateTime.Now.Subtract(new TimeSpan(0, LAST_CONNECT_TIME_MIN_MAX, 0));

            await context.WorkerLogs
                .Where(wl => wl.LastConnectTime < lowerBoundTime)
                .ExecuteUpdateAsync(spc => spc.SetProperty(wl => wl.State, WorkerLogState.Disconnected), cancellationToken: cancellationToken);
            
            await _periodicTimer.WaitForNextTickAsync(cancellationToken);
        }
    }
    
    /// <inheritdoc /> 
    public Task StopAsync(CancellationToken cancellationToken)=> Task.CompletedTask;
    
}