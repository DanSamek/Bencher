namespace Worker;

public static class UserOptionsLoader
{
    public record UserOptions(int NumberOfThreads);
    
    /// <summary>
    /// Loads users params for the runner.
    /// </summary>
    public static UserOptions LoadParams()
    {
        #if DEBUG
        return new UserOptions(4);
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