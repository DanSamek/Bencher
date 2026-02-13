using System.ComponentModel.DataAnnotations;

namespace WebApplication.Data.Models;

/// <summary>
/// Error entity.
/// </summary>
public class TestError : WorkerError
{
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
    
    /// <summary>
    /// Id of the WorkerLog, where error happened.
    /// </summary>
    [Required]
    public required int WorkerLogId { get; set; }
}