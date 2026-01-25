using System.Collections.Concurrent;

namespace Worker.ThreadSplitManager;

/// <summary>
/// Thread split management.
/// Runs multiple instances of the runner.
/// Thread split is used in the scenario, where we have less workers than number of tests
///     - idea is to split worker threads automatically to ensure, that all test are running
/// NOTE, if one thread has problems with the communication, it will end everything!
/// </summary>
public class ThreadSplitManager
{
    private readonly RunnerOptions _runnerOptions;
    private readonly Notifier _notifier;

    private const int N_ITERATIONS = 5;
    
    private readonly ManualResetEvent _resetEvent = new ManualResetEvent(true);
    private readonly ConcurrentQueue<int> _availableThreads = new ConcurrentQueue<int>();
    private readonly Communication _communication; 
    private readonly IClientFactory _clientFactory;
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public ThreadSplitManager(RunnerOptions runnerOptions, Notifier notifier, Communication communication, IClientFactory clientFactory)
    {
        _runnerOptions = runnerOptions;
        _notifier = notifier;
        _communication = communication;
        _clientFactory = clientFactory;
    }

    /// <summary>
    /// Runs thread split.
    /// </summary>
    public void Run()
    {
        _availableThreads.Enqueue(_runnerOptions.NumberOfThreads);
        while (true)
        {
            _resetEvent.WaitOne();
            var threadsPerRunner = GetThreadsPerRunner();
            _resetEvent.Reset();
            
            foreach (var runnerThreads in threadsPerRunner)
            {
                var result = ThreadPool.QueueUserWorkItem(_ =>
                {
                    Task.Run(async () =>
                    {
                        var runnerOptions = _runnerOptions with { NumberOfThreads = runnerThreads };
                        var runner = new Runner(runnerOptions, _notifier, _clientFactory);
                        await runner.Run(N_ITERATIONS);

                        _availableThreads.Enqueue(runnerThreads);
                        _resetEvent.Set();
                    });
                });
                
                if (result) continue;
                
                _availableThreads.Enqueue(runnerThreads);
            }
        }
    }
    
    private IReadOnlyList<int> GetThreadsPerRunner()
    {
        var maxThreadsForTest = _communication.MaxThreadsForTest();
        var pausedTests = _communication.TotalPausedTests();
        
        var result = ThreadSplit.Split(_availableThreads, maxThreadsForTest, pausedTests);
        return result;
    }
}