
using System.ComponentModel.DataAnnotations;

namespace WebApplication.Data;

/// <summary>
/// Worker log entity.
/// We don't store workers as entities. 
/// </summary>
public class WorkerLog : DoId
{
    /// <summary>
    /// Time of the connection for the current workload.
    /// </summary>
    [Required]
    public required DateTime ConnectTime { get; set; }
    
    /// <summary>
    /// Number of games that will be played on the worker.
    /// </summary>
    [Required]
    public required int NumberOfGames { get; set; }
    
    /// <summary>
    /// MAC address of the worker.
    /// </summary>
    [Required]
    public required int Mac { get; set; }
}