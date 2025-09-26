namespace WebApplication.Data.Models;

/// <summary>
/// State for the <see cref="Test" />.
/// </summary>
public enum TestState : byte
{
    Paused,
    Running,
    Finished,
    Stopped
}

public static class TestStateExtensions
{
    /// <summary>
    /// If test is not running.
    /// </summary>
    public static bool Running(this TestState state) => state == TestState.Running;
}