namespace Shared.Dtos.Responses;

/// <summary>
/// Response of the <see cref="WorkerController.TotalPausedTests" /> 
/// </summary>
public class TotalPausedTestsDto : ResponseBase
{
    public int Count { get; set; }
}