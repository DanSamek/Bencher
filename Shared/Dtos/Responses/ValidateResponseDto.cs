namespace Shared.Dtos.Responses;

/// <summary>
/// Response of the <see cref="WorkerController.Validate" /> 
/// </summary>
public class ValidateResponseDto : ResponseBase
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public ValidateResponseDto(string username) => Username = username;
    
    /// <summary>
    /// Username of the user.
    /// </summary>
    public string Username { get; init; } 
}