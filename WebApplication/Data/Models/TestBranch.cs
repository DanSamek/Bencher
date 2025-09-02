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
    [MaxLength(30)]
    public required string Name { get; set; }
    
    /// <summary>
    /// Engine for a branch. 
    /// </summary>
    [Required]
    public required Engine Engine { get; set; }
    
    /// <summary>
    /// Test for a branch.
    /// </summary>
    public Test? TestBranchOf { get; set; }
    
    /// <summary>
    /// Test for a branch.
    /// </summary>
    public Test? BaseBranchOf { get; set; }
}