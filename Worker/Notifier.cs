
namespace Worker;

/// <summary>
/// Class that is periodically calling /running-test,
/// because all worker logs are "watched" by the server, if they are active.  
/// </summary>
public class Notifier
{
    private readonly Communication _communication;
    private readonly HashSet<int> _connectionIds = new HashSet<int>();
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
    private readonly PeriodicTimer _timer = new PeriodicTimer(new TimeSpan(0,0,15));
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public Notifier(RunnerOptions runnerOptions)
    {
        _communication = new Communication(runnerOptions);
    }
    
    public async Task AddNotifyRunningTest(int connectionId)
    {
        await _semaphoreSlim.WaitAsync();
        _connectionIds.Add(connectionId);
        _semaphoreSlim.Release();
    }
    
    public async Task RemoveNotifyRunningTest(int connectionId)
    {
        await _semaphoreSlim.WaitAsync();
        _connectionIds.Remove(connectionId);
        _semaphoreSlim.Release();
    }
    
    public async Task Run()
    {
        while (true)
        {
            await _semaphoreSlim.WaitAsync();
            foreach (var connectionId in _connectionIds)
            {
                _communication.RunningTest(connectionId);
            }
            _semaphoreSlim.Release();
            await _timer.WaitForNextTickAsync();
        }
    }
}