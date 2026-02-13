using System.ComponentModel.DataAnnotations;
using System.Text;

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
    /// NOTE: Better option here would be to make lazy loading.
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
    
    /// <summary>
    /// Converts build script to the string. 
    /// </summary>
    public string GetBuildScriptString() => Encoding.ASCII.GetString(BuildScript);
    
    /// <summary>
    /// Helper function for string buildscript conversion to the byte array. 
    /// </summary>
    public static byte[] GetBuildScriptBytes(string buildScript) => Encoding.ASCII.GetBytes(buildScript);

}