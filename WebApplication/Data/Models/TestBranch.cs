using System.ComponentModel.DataAnnotations;

namespace WebApplication.Data;

/// <summary>
/// Testing branch entity.
/// </summary>
public class TestBranch : DoId
{
    /// <summary>
    /// Version bench.
    /// </summary>
    [Required] 
    public required int Bench { get; set; }
    
    /// <summary>
    /// Git branch name. 
    /// </summary>
    [Required]
    public required string Name { get; set; }
}