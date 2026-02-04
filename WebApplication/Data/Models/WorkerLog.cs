
using System.ComponentModel.DataAnnotations;

namespace WebApplication.Data.Models;

/// <summary>
/// Worker log entity.
/// Note: Id is also used as a in the <see cref="API.WorkerController"/> ConnectionId.
/// </summary>
public class WorkerLog : DoId
{
    public const int MAX_NAME_LENGTH = 128;
    /// <summary>
    /// Machine name.
    /// </summary>
    [Required]
    [MaxLength(MAX_NAME_LENGTH)]
    public required string Name { get; set; }
    
    /// <summary>
    /// Current state.
    /// </summary>
    [Required]
    public required WorkerLogState State { get; set; }
    
    /// <summary>
    /// Initial state of the test.
    /// TODO Why did i added that??
    /// </summary>
    [Required]
    public required InitialTestState InitialTestState { get; set; }
    
    /// <summary>
    /// Time of the connection for the current workload.
    /// </summary>
    [Required]
    public required DateTime ConnectTime { get; set; }
    
    /// <summary>
    /// Last connect time of the worker.
    /// </summary>
    public DateTime? LastConnectTime { get; set; }
    
    /// <summary>
    /// Number of games that was played on the worker.
    /// </summary>
    [Required]
    public required int NumberOfGames { get; set; }
    
    /// <summary>
    /// Number of games that will be played on the worker.
    /// </summary>
    [Required]
    public required int TotalNumberOfGames { get; set; }
    
    /// <summary>
    /// Number of threads of the worker. 
    /// </summary>
    [Required]
    public required int NumberOfThreads { get; set; }
    
    /// <summary>
    /// MAC address of the worker.
    /// </summary>
    [Required]
    [MaxLength(17)]
    public required string Mac { get; set; }

    /// <summary>
    /// All errors, that happened for this log.
    /// TODO, this is going to be only one!
    /// </summary>
    public List<TestError> Errors { get; set; } = [];
    
    /// <summary>
    /// User that belongs to a log.
    /// </summary>
    [Required]
    public required ApplicationUser User { get; set; }
    
    /// <summary>
    /// Test that belongs to a log. 
    /// </summary>
    [Required]
    public required Test Test { get; set; }

    /// <summary>
    /// Summary identifier of the worker.
    /// </summary>
    /// <returns></returns>
    public string Identifier() => $"{Id}-{Name}-{NumberOfThreads}";

    /// <summary>
    /// Sets last connect time to now.
    /// </summary>
    public void SetLastConnectTimeNow()
        => LastConnectTime = DateTime.UtcNow;
}