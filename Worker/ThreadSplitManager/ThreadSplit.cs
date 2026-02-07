using System.Collections.Concurrent;

namespace Worker.ThreadSplitManager;

/// <summary>
/// Logic for spliting threads - how will be threads split.
/// </summary>
public static class ThreadSplit
{
    private const int N_PARALLEL_GAMES_RUNNER = 8;
    
    /// <summary>
    /// Splits available threads based on maximum number of threads and number of paused tests.
    /// </summary>
    public static IReadOnlyList<int> Split(ConcurrentQueue<int> returnedThreads, 
                                           int maxThreadsForTest,
                                           int pausedTests)
    {
        var availableThreads = 0;
        while (returnedThreads.TryDequeue(out var threads)) availableThreads += threads;
        if (availableThreads == 0) return [];
        if (pausedTests == 0) return [availableThreads];
        
        var parallelGamesPerRunner = N_PARALLEL_GAMES_RUNNER;
        int minimumThreadsPerRunner;
        while (true)
        {
            minimumThreadsPerRunner = Math.Min(maxThreadsForTest * parallelGamesPerRunner, availableThreads);
            var instancesToRun = availableThreads / minimumThreadsPerRunner;
            
            if (instancesToRun > pausedTests && instancesToRun != 1)
            {
                parallelGamesPerRunner *= 2;
                continue;
            }
            break;
        }
        
        var result = new List<int>();
        var nThreads = availableThreads;
        while (nThreads > minimumThreadsPerRunner)
        {
            result.Add(minimumThreadsPerRunner);
            nThreads -= minimumThreadsPerRunner;
        }
        result.Add(nThreads);
        return result;
    }
    
}