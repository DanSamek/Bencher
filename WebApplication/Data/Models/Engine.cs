using System.ComponentModel.DataAnnotations;

namespace WebApplication.Data;

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
    /// Build script for the windows.
    /// It can be same as the <see cref="BuildScriptLinux"/>.
    /// </summary>
    [Required]
    [MaxLength(1024)]
    public required string BuildScriptWindows { get; set; }
    
    /// <summary>
    /// Build script for the linux.
    /// It can be same as the <see cref="BuildScriptWindows"/>.
    /// </summary>
    [Required]
    [MaxLength(1024)]
    public required string BuildScriptLinux { get; set; }
    
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