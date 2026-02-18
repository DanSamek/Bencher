using log4net;
using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.API;

/// <summary>
/// Service, that will watch all running worker logs and sets to state Disconnected,
/// if last now - connection time of the worker log is higher than  LAST_CONNECT_TIME_MIN_MAX.
/// </summary>
public class WorkerLogWatcher : BackgroundService
{
    private const int LAST_CONNECT_TIME_MINUTES_MAX = 1;
    private const int WATCHER_PERIOD_MINUTES = 1;
    
    private readonly PeriodicTimer _periodicTimer = new PeriodicTimer(new TimeSpan(0, 0, WATCHER_PERIOD_MINUTES, 0));
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private static readonly ILog _logger = LogManager.GetLogger(typeof(WorkerLogWatcher));
    
    /// <summary>
    /// .Ctor 
    /// </summary>
    public WorkerLogWatcher(IServiceScopeFactory serviceScopeFactory) => _serviceScopeFactory = serviceScopeFactory;

    
    /// <inheritdoc /> 
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (true)
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<ITestService>();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var lowerBoundTime = DateTime.UtcNow.Subtract(new TimeSpan(0, LAST_CONNECT_TIME_MINUTES_MAX, 0));

                var toDisconnectLogs = context.WorkerLogs
                    .Where(wl => wl.LastConnectTime < lowerBoundTime && wl.State == WorkerLogState.Active);

                var testIdsMaybeNoWorkers = toDisconnectLogs
                    .Select(wl => wl.Test.Id)
                    .ToHashSet();

                await toDisconnectLogs
                    .ExecuteUpdateAsync(spc => spc.SetProperty(wl => wl.State, WorkerLogState.Disconnected),
                        cancellationToken: stoppingToken);
                
                foreach (var testId in testIdsMaybeNoWorkers)
                {
                    await service.SetPausedIfNoActiveWorkers(testId);
                }

                await _periodicTimer.WaitForNextTickAsync(stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message, ex);
        }
    }
}