namespace Worker.TestProcessors;

/// <summary>
/// Interface for the test processor.
/// </summary>
public interface ITestProcessor<TOut>
{
    /// <summary>
    /// Processes a test with arguments.
    /// </summary>
    public Task<TOut> Process();
}