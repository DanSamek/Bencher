using Shared;

namespace WebApplication.API;

public static class TotalNumberGamesCalculator
{
    /// <summary>
    /// Calculates how many games should be played on the worker.
    /// </summary>
    public static int Calculate(int numberOfWorkerThreads, int numberOfThreadsTest)
    {
        if (numberOfWorkerThreads < numberOfThreadsTest) throw new Exception($"{nameof(numberOfWorkerThreads)} is smaller than {nameof(numberOfThreadsTest)}.");
        
        var result = (numberOfWorkerThreads / numberOfThreadsTest) * Constants.GAME_THREAD_COUNT_MULTIPLIER;
        return result;
    }
}