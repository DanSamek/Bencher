using System.ComponentModel.DataAnnotations;

namespace WebApplication.Data.Models;

/// <summary>
/// Engine entity
/// </summary>
public class Engine : DoId
{
    /// <summary>
    /// Engine name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
    
    /// <summary>
    /// Base git url for the engine.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public required string GitUrl { get; set; }
    
    /// <summary>
    /// Build script for the linux.
    /// </summary>
    [Required]
    public required byte[] BuildScript { get; set; }
    
    /// <summary>
    /// User, that engine has.
    /// </summary>
    [Required]
    public required ApplicationUser User { get; set; }
    
    /// <summary>
    /// All tests for the engine.
    /// </summary>
    [Required]
    public required List<Test> Tests { get; set; } = [];
    
    /// <summary>
    /// All branches for the engine.
    /// </summary>
    [Required]
    public required List<TestBranch> Branches { get; set; } = [];
}