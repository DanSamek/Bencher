namespace WebApplication.API.Dtos.Responses;

public class RunningResponseBase : ResponseBase
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public RunningResponseBase(bool running) => Running = running;
    
    /// <summary>
    /// If test is running.
    /// </summary>
    public bool Running { get; init; }
}