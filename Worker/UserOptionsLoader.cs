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
        
        var result = new UserOptions(numberOfThreads, splitThreads);
        return result;
    }
}
