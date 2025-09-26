namespace WebApplication.API.Dtos.Responses;

public class ResultsResponseDto : RunningResponseBase
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public ResultsResponseDto(bool running) : base(running) { }
}