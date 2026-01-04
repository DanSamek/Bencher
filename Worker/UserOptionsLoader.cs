namespace Worker;

public static class UserOptionsLoader
{
    /// <summary>
    /// Options, that will be loaded from the user.
    /// </summary>
    /// <param name="NumberOfThreads">Number of threads to use.</param>
    /// <param name="TrySplitThreads">
    ///     If threads can be split.
    ///     For example if worker has 64 threads, it will be split to [16,16,16,16] threads.
    ///     NOTE: This can be used only for workers with big amount of threads.
    /// </param>
    public record UserOptions(int NumberOfThreads, bool TrySplitThreads);
    
    /// <summary>
    /// Loads users params for the runner.
    /// </summary>
    public static UserOptions LoadParams()
    {
        #if DEBUG
        return new UserOptions(4, false);
        #else
        
        Console.WriteLine($"Number of threads to use (maximum is {Environment.ProcessorCount}): ");
        var numberOfThreads = int.Parse(Console.ReadLine()!);
        if (numberOfThreads > Environment.ProcessorCount)
        {
            Console.WriteLine($"Number of threads is set to {Environment.ProcessorCount}.");
            numberOfThreads = Environment.ProcessorCount;
        }
        
        var result = new UserOptions(numberOfThreads);
        return result;
        #endif
    }
}