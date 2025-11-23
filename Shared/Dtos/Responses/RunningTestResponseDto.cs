namespace Shared.Dtos.Responses;

/// <summary>
/// Response of the <see cref="WorkerController.RunningTest" /> 
/// </summary>
public class RunningTestResponseDto : RunningResponseBase
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public RunningTestResponseDto(bool running) : base(running) {}
}