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
    public required string Name { get; set; }
    
    /// <summary>
    /// Test description.
    /// </summary>
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
    public required string TimeManagement { get; set; }
    
    /// <summary>
    /// Current state of the test.
    /// </summary>
    [Required]
    public required TestState State { get; set; }
}
