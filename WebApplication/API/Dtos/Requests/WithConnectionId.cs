using System.ComponentModel.DataAnnotations;

namespace WebApplication.API.Dtos;

/// <summary>
/// Base for all requests with ConnectionId.
/// </summary>
public class WithConnectionId
{
    /// <summary>
    /// Also as <see cref="Data.Models.WorkerLog.Id" /> 
    /// </summary>
    [Required(ErrorMessage = "Connection id is required.")]
    public required int ConnectionId { get; set; }
}