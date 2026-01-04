namespace Worker.TestProcessors;

/// <summary>
/// Interface for the test processor.
/// </summary>
/// <typeparam name="TArgs">Arguments for the processor.</typeparam>
public interface ITestProcessor<in TArgs, TOut>
{
    /// <summary>
    /// Processes a test with arguments.
    /// </summary>
    public Task<TOut> Process(TArgs args);
}