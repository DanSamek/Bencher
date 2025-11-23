using Shared.CustomAttributes;

namespace Shared.Dtos.Requests;
/// <summary>
/// Request body for WorkerController autobench.  
/// </summary>
public class AutobenchDto : WithConnectionId
{
    /// <summary>
    /// Worker's bench result.
    /// </summary>
    [Min(0, ErrorMessage = "Invalid autobench value")]
    public required int Autobench { get; set; }
}