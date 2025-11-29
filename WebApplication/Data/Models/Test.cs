using System.ComponentModel.DataAnnotations;
using WebApplication.Extensions;

namespace WebApplication.Data.Models;

/// <summary>
/// Test entity.
/// </summary>
public class Test : DoId
{
    /// <summary>
    /// Calculates thread scale.
    /// Note, we can call it in .Ctor, but TimeManagement can be null.
    /// </summary>
    public void CalculateThreadScale() => ThreadScale = NumberOfThreads * TimeManagement.Seconds(); 
    
    /// <summary>
    /// Test name.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public required string Name { get; set; }
    
    /// <summary>
    /// Test description.
    /// </summary>
    [MaxLength(1024)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Time, when was test created.
    /// </summary>
    [Required]
    public required DateTime Created { get; set; }
    
    /// <summary>
    /// Time, when test ended. 
    /// </summary>
    public DateTime? Ended { get; set; }
    
    /// <summary>
    /// Priority of the test.
    /// Higher priority = will be tested first.
    /// </summary>
    [Required]
    public required int Priority { get; set; }
    
    /// <summary>
    /// Number of threads that will be used for the test.
    /// </summary>
    [Required]
    public required int NumberOfThreads { get; set; }
    
    /// <summary>
    /// Size of the hashtable that will be used for the test.
    /// </summary>
    [Required]
    public required int HashSize { get; set; }
    
    /// <summary>
    /// Timemanagement that will be used for the test.
    /// </summary>
    [Required]
    [MaxLength(30)]
    public required string TimeManagement { get; set; }
    
    /// <summary>
    /// Current state of the test.
    /// </summary>
    [Required]
    public required TestState State { get; set; }

    /// <summary>
    /// Penta that belongs to a test.
    /// </summary>
    public Penta Penta { get; set; } = null!;
    
    /// <summary>
    /// SPRT settings that belongs to a test.
    /// </summary>
    [Required]
    public required SprtSettings Settings { get; set; }
    
    /// <summary>
    /// Opening book, that uses a test.
    /// </summary>
    [Required]
    public required OpeningBook OpeningBook { get; set; }

    /// <summary>
    /// All errors for a test.
    /// </summary>
    [Required]
    public required List<TestError> Errors { get; set; } = [];

    /// <summary>
    /// All worker logs for a test.
    /// </summary>
    [Required]
    public required List<WorkerLog> WorkerLogs { get; set; } = [];
    
    /// <summary>
    /// Engine that belongs to this test. 
    /// </summary>
    [Required]
    public required Engine Engine { get; set; }
    
    /// <summary>
    /// Base branch for a test.
    /// </summary>
    [Required]
    public TestBranch BaseBranch { get; set; } = null!;
    
    /// <summary>
    /// Id of the base branch.
    /// </summary>
    public int BaseBranchId { get; set; }

    /// <summary>
    /// Branch with (for example) new heuristic. 
    /// </summary>
    [Required]
    public TestBranch TestBranch { get; set; } = null!;
    
    /// <summary>
    /// Id of the test branch.
    /// </summary>
    public int TestBranchId { get; set; }
    
    /// <summary>
    /// User, that created test.
    /// </summary>
    [Required]
    public required ApplicationUser User { get; set; }
    
    /// <summary>
    /// Thread scale of the test.
    /// Used in the TestQueue - for which test we should add worker. 
    /// </summary>
    [Required]
    public int ThreadScale { get; set; }
    
    /// <summary>
    /// State of the autobench.
    /// Technically we don't need <see cref="Autobenched"/>, but it will be faster - no additional db queries needed. 
    /// </summary>
    public AutobenchState? AutobenchState { get; set; }
    
    /// <summary>
    /// If test is autobenched.
    /// </summary>
    [Required]
    public required bool Autobenched { get; set; }
    
    /// <summary>
    /// Expected NPS at the worker.
    /// At worker we will scale time based on expected NPS.
    /// NPS = nodes per second
    /// </summary>
    [Required]
    public required int ExpectedNps { get; set; }
    
    /// <summary>
    /// Gets total number of active worker threads for the test.
    /// <see cref="WorkerLogs"/> has to be included (loaded from database).
    /// </summary>
    public int ActiveWorkerThreadCount()
    {
        var result = WorkerLogs
            .Where(wl => wl.State == WorkerLogState.Active)
            .Where(wl => wl.NumberOfGames != wl.TotalNumberOfGames)
            .Sum(wl => wl.NumberOfThreads);
        
        return result;
    }
}