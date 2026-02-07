namespace Worker.TestProcessors.GameTestProcessor;

public static class GamePairCalculator
{
    /// <summary>
    /// Calculates how many pairs are needed to complete before sending it to a webapp. 
    /// </summary>
    public static int CalculatePairsNeeded(string timeManagement, int numberOfThreads, int processorThreads)
    {
        var (seconds, _) = timeManagement.Tm();
        
        var log = (int)Math.Log2(seconds * 1.0 / 30 + numberOfThreads);
        var perProcessor = Math.Max(4 - log, 1);
        return perProcessor * (processorThreads / numberOfThreads);
    }
}