namespace WebApplication.Data;

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