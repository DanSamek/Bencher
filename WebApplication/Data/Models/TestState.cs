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