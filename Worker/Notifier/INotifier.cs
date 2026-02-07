namespace Worker.Notifier;

/// <summary>
/// Interface for the notifier.
/// </summary>
public interface INotifier
{
    /// <summary>
    /// Ensures that notifier will periodically call /running-test to the web-app  with specified connectionid.
    /// </summary>
    public Task AddNotifyRunningTest(int connectionId);

    /// <summary>
    /// Ensures that no longer will be periodically called /running-test to the web-app with specified connectionid.
    /// </summary>
    public Task RemoveNotifyRunningTest(int connectionId);

    /// <summary>
    /// If current test (under connection id) is still running.
    /// </summary>
    public bool IsTestStillRunning(int connectionId);

    /// <summary>
    /// Runs notifier.
    /// </summary>
    public Task Run();
    
    /// <summary>
    /// Ensures, that notifier will be stopped.
    /// </summary>
    void EnsureStopped();
}