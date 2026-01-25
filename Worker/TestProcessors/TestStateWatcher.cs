namespace Worker.TestProcessors;

/// <summary>
/// Helper class if test is still running.
/// </summary>
public class TestStateWatcher
{
    private readonly Notifier _notifier;
    private readonly int _connectionId;
    private readonly PeriodicTimer _timer = new PeriodicTimer(new TimeSpan(0,0,15));
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        
    /// <summary>
    /// .Ctor
    /// </summary>
    public TestStateWatcher(Notifier notifier, int connectionId)
    {
        _notifier = notifier;
        _connectionId = connectionId;
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