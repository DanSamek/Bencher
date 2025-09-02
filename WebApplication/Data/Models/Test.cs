using System.ComponentModel.DataAnnotations;

namespace WebApplication.Data;

/// <summary>
/// Test entity.
/// </summary>
public class Test : DoId
{
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
    /// Priority of the test.
    /// Higher priority = will be tested first.
    /// </summary>
    [Required]
    public required int Priority { get; set; }
    
    /// <summary>
    /// If test is autobenched.
    /// </summary>
    [Required]
    public required bool Autobenched { get; set; }
    
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
    [Required]
    public required Penta Penta { get; set; }
    
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
    public required List<Error> Errors { get; set; } = [];

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
    public required TestBranch BaseBranch { get; set; }
    
    /// <summary>
    /// Id of the base branch.
    /// </summary>
    public int BaseBranchId { get; set; }
    
    /// <summary>
    /// Branch with (for example) new heuristic. 
    /// </summary>
    [Required]
    public required TestBranch TestBranch { get; set; }
    
    /// <summary>
    /// Id of the test branch.
    /// </summary>
    public int TestBranchId { get; set; }
    
    /// <summary>
    /// User, that created test.
    /// </summary>
    [Required]
    public required ApplicationUser User { get; set; }

}
