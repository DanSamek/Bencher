
namespace Worker.Notifier;

/// <summary>
/// Class that is periodically calling /running-test,
/// because all worker logs are "watched" by the server, if they are active.
/// </summary>
public class Notifier : INotifier
{
    private readonly ICommunication _communication;
    private readonly HashSet<int> _connectionIds = new HashSet<int>();
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
    private Dictionary<int, bool> _testStates = new();
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly PeriodicTimer _timer;
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public Notifier(ICommunication communication, TimeSpan? notifyWaitTick = null)
    {
        _communication = communication;
        _timer = new PeriodicTimer(notifyWaitTick ?? new TimeSpan(0, 0, 15));
    }
    
    /// <inheritdoc /> 
    public async Task AddNotifyRunningTest(int connectionId)
    {
        await _semaphoreSlim.WaitAsync();
        _connectionIds.Add(connectionId);
        _semaphoreSlim.Release();

        _communication.RunningTest(connectionId);
    }
    
    /// <inheritdoc /> 
    public async Task RemoveNotifyRunningTest(int connectionId)
    {
        await _semaphoreSlim.WaitAsync();
        _connectionIds.Remove(connectionId);
        _semaphoreSlim.Release();
    }
    
    /// <inheritdoc /> 
    public bool IsTestStillRunning(int connectionId)
    {
        var result = _testStates.GetValueOrDefault(connectionId, true);
        return result;
    }
    
    /// <inheritdoc /> 
    public async Task Run()
    {
        while (!_cts.IsCancellationRequested)
        {
            await _semaphoreSlim.WaitAsync(_cts.Token);
            
            var newTestStates = new Dictionary<int, bool>();
            foreach (var connectionId in _connectionIds)
            {
                var response = _communication.RunningTest(connectionId);
                newTestStates.Add(connectionId, response?.Running ?? false);
            }
            
            _semaphoreSlim.Release();
            
            _testStates = newTestStates;
            await _timer.WaitForNextTickAsync(_cts.Token);
        }
    }

    /// <inheritdoc /> 
    public void EnsureStopped() => _cts.Cancel();
}