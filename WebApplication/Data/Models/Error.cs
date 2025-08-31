using System.ComponentModel.DataAnnotations;

namespace WebApplication.Data;

/// <summary>
/// Error entity.
/// </summary>
public class Error : DoId
{
    /// <summary>
    /// Time when the error happened.
    /// </summary>
    [Required]
    public required DateTime Time { get; set; }
    
    /// <summary>
    /// Log of the error.
    /// </summary>
    [Required]
    public required string Log { get; set; }
}