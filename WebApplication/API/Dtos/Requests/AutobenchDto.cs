namespace WebApplication.API.Dtos.Requests;

/// <summary>
/// Request body for <see cref="WorkerController.Autobench" />  
/// </summary>
public class AutobenchDto : WithConnectionId
{
    /// <summary>
    /// Worker's bench result.
    /// </summary>
    [Min(0, ErrorMessage = "Invalid autobench value")]
    public required int Autobench { get; set; }
}