
namespace Worker;

/// <summary>
/// Class that is periodically calling /running-test,
/// because all worker logs are "watched" by the server, if they are active.
/// </summary>
public class Notifier
{
    private readonly ICommunication _communication;
    private readonly HashSet<int> _connectionIds = new HashSet<int>();
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
    private readonly PeriodicTimer _timer = new PeriodicTimer(new TimeSpan(0,0,15));
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public Notifier(ICommunication communication)
    {
        _communication = communication;
    }
    
    public async Task AddNotifyRunningTest(int connectionId)
    {
        await _semaphoreSlim.WaitAsync();
        _connectionIds.Add(connectionId);
        _semaphoreSlim.Release();

        _communication.RunningTest(connectionId);
    }
    
    public async Task RemoveNotifyRunningTest(int connectionId)
    {
        await _semaphoreSlim.WaitAsync();
        _connectionIds.Remove(connectionId);
        _semaphoreSlim.Release();
    }
    
    private Dictionary<int, bool> _testStates = new();
    public bool IsTestStillRunning(int connectionId)
        => _testStates.GetValueOrDefault(connectionId, true);
    
    public async Task Run()
    {
        while (true)
        {
            await _semaphoreSlim.WaitAsync();
            
            var newTestStates = new Dictionary<int, bool>();
            foreach (var connectionId in _connectionIds)
            {
                var response = _communication.RunningTest(connectionId);
                newTestStates.Add(connectionId, response?.Running ?? false);
            }
            
            _semaphoreSlim.Release();
            
            _testStates = newTestStates;
            await _timer.WaitForNextTickAsync();
        }
    }
}