namespace Shared.Dtos.Responses;

/// <summary>
/// Response of the <see cref="WorkerController.MaxThreadsForTest" /> 
/// </summary>
public class MaxThreadsForTestDto : ResponseBase
{
    public int MaximumThreads { get; set; }
}