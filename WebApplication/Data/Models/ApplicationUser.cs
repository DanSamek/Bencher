using Microsoft.AspNetCore.Identity;

namespace WebApplication.Data;

/// <summary>
/// User entity.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Access token for the API calls from Worker.
    /// </summary>
    public string? AccessToken { get; set; }
}