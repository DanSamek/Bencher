using System.ComponentModel.DataAnnotations;

namespace WebApplication.Data.Models;

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
    /// Worker app will upload xxx.txt
    /// </summary>
    [Required]
    public required byte[] Log { get; set; }
    
    /// <summary>
    /// Test, where this error happened.
    /// </summary>
    [Required]
    public required Test Test { get; set; }
    
    /// <summary>
    /// Worker information.
    /// </summary>
    [Required]
    public required WorkerLog WorkerLog { get; set; }
    
}