using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.Requests;

/// <summary>
/// Request body for <see cref="WorkerController.TestError" />  
/// </summary>
public class TestErrorDto : WithConnectionId
{
    /// <summary>
    /// File with the entire log.
    /// </summary>
    [Required(ErrorMessage = "Log file is required.")]
    public required byte[] Log { get; set; }
}