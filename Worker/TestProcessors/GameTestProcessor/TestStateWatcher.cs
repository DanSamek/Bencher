using Worker.Notifier;

namespace Worker.TestProcessors.GameTestProcessor;

/// <summary>
/// Helper class if test is still running.
/// </summary>
public class TestStateWatcher
{
    private readonly INotifier _notifier;
    private readonly int _connectionId;
    private readonly PeriodicTimer _timer;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        
    /// <summary>
    /// .Ctor
    /// </summary>
    public TestStateWatcher(INotifier notifier, int connectionId, TimeSpan? stillRunningWaitTick = null)
    {
        _notifier = notifier;
        _connectionId = connectionId;
        _timer = new PeriodicTimer(stillRunningWaitTick ?? new TimeSpan(0, 0, 15));
    }

    public bool Running { get; private set; } = true;

    public async Task Watch()
    {
        while (!_cts.IsCancellationRequested)
        {
            Running = _notifier.IsTestStillRunning(_connectionId);
            if (!Running) break;
            
            await _timer.WaitForNextTickAsync(_cts.Token);
        }
    }
    
    public void EnsureStopped() => _cts.Cancel();
}