namespace WebApplication.Data.Models;

/// <summary>
/// State for the <see cref="Test" />.
/// </summary>
public enum TestState : byte
{
    Paused,
    Autobenched,
    Running,
    Finished,
    Stopped
}

public static class TestStateExtensions
{
    /// <summary>
    /// If test is not running.
    /// </summary>
    public static bool Running(this TestState state) => state is TestState.Running or TestState.Autobenched;
}