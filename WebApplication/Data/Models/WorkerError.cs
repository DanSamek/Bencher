using System.ComponentModel.DataAnnotations;

namespace WebApplication.Data.Models;

/// <summary>
/// Error class for workers
///     - used for errors without a test - for example missing references,..
/// </summary>
public class WorkerError : DoId
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
    public ErrorContent Log { get; set; } = null!;
}