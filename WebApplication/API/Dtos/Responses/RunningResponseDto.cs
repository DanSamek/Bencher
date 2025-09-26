namespace WebApplication.API.Dtos.Responses;

public class RunningResponseDto : RunningResponseBase
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public RunningResponseDto(bool running) : base(running) {}
}