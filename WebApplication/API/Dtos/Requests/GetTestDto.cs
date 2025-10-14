using System.ComponentModel.DataAnnotations;

namespace WebApplication.API.Dtos.Requests;

/// <summary>
/// Request body for <see cref="WorkerController.GetTest" />  
/// </summary>
public class GetTestDto
{
    /// <summary>
    /// If required test is autobench.
    /// </summary>
    [Required(ErrorMessage = "Autobench is required.")]
    public required bool Autobench { get; set; }
    
    /// <summary>
    /// Mac of the worker.
    /// </summary>
    [Required]
    [Mac(ErrorMessage = "Invalid MAC address.")]
    public required string Mac { get; set; }
    
    /// <summary>
    /// Name of the worker (device name for example).
    /// </summary>
    [Required]
    public required string Name { get; set; }
    
    /// <summary>
    /// Number of threads of the worker.
    /// </summary>
    [Min(1)]
    public required int NumberOfThreads { get; set; }
}