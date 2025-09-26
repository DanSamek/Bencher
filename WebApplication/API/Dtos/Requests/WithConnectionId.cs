using System.ComponentModel.DataAnnotations;

namespace WebApplication.API.Dtos;

/// <summary>
/// Base for all requests with ConnectionId.
/// </summary>
public class WithConnectionId
{
    [Required(ErrorMessage = "Connection id is required.")]
    public required int ConnectionId { get; set; }
}