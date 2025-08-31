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
    public required string Name { get; set; }
    
    /// <summary>
    /// Base git url for the engine.
    /// </summary>
    [Required]
    public required string GitUrl { get; set; }
    
    /// <summary>
    /// Build script for the windows.
    /// It can be same as the <see cref="BuildScriptLinux"/>.
    /// </summary>
    [Required]
    public required string BuildScriptWindows { get; set; }
    
    /// <summary>
    /// Build script for the linux.
    /// It can be same as the <see cref="BuildScriptWindows"/>.
    /// </summary>
    [Required]
    public required string BuildScriptLinux { get; set; }
}