using System.ComponentModel.DataAnnotations;

namespace WebApplication.API.Dtos.Requests;

/// <summary>
/// Request body for <see cref="WorkerController.Error" />  
/// </summary>
public class ErrorDto : WithConnectionId
{
    /// <summary>
    /// File with the entire log.
    /// </summary>
    [Required(ErrorMessage = "Log file is required.")]
    public required IFormFile Log { get; set; }
}