namespace Worker.UI;

public static class UserOptionsLoader
{
    /// <summary>
    /// Minimum number of threads required for the thread split algorithm. 
    /// </summary>
    #if DEBUG
    private const int MIN_THREAD_SPLIT = 0;
    #else
    private const int MIN_THREAD_SPLIT = 16;
    #endif
    /// <summary>
    /// Options, that will be loaded from the user.
    /// </summary>
    /// <param name="NumberOfThreads">Number of threads to use.</param>
    /// <param name="TrySplitThreads"><see cref="ThreadSplitManager.ThreadSplitManager" /></param>
    public record UserOptions(int NumberOfThreads, bool TrySplitThreads);
    
    /// <summary>
    /// Loads users params for the runner.
    /// </summary>
    public static UserOptions LoadParams()
    {
        var valid = false;
        var numberOfThreads = 0;
        var splitThreads = false;
        while (!valid)  
        {
            Console.WriteLine($"Number of threads to use (maximum is {Environment.ProcessorCount}): ");
            valid = int.TryParse(Console.ReadLine(), out numberOfThreads);
            
            if (numberOfThreads <= Environment.ProcessorCount) continue;
            Console.WriteLine($"Number of threads is set to {Environment.ProcessorCount}.");
            numberOfThreads = Environment.ProcessorCount;
        }

        if (numberOfThreads >= MIN_THREAD_SPLIT)
        {
            valid = false;
            while (!valid)
            {
                Console.WriteLine("Try split threads [T/F]:");
                var splitThreadsString = Console.ReadLine();
                if (splitThreadsString is null || splitThreadsString.Length != 1) continue;

                var splitThreadsChar = char.ToLower(splitThreadsString[0]); 
                splitThreads = splitThreadsChar switch
                {
                    't' => true,
                    _ => false
                };
                
                valid = splitThreadsChar switch
                {
                    't' or 'f' => true,
                    _ => false
                };
            }    
        }
        
        var result = new UserOptions(numberOfThreads, splitThreads);
        return result;
    }
}
