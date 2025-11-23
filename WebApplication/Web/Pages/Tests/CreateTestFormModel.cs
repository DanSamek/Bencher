using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Shared.CustomAttributes;

namespace WebApplication.Components.Pages.Tests;

public class CreateTestFormModel
{
    [Required]
    [DisplayName("Test name")]
    public string TestName { get; set; } = null!;
    
    public string? Description { get; set; }
    
    [Required]
    [DisplayName("Branch name")]
    public string BaseBranchName { get; set; } = null!;
        
    [Min(0)]
    [DisplayName("Bench")]
    public int BaseBranchBench { get; set; }
    
    [Required]
    [DisplayName("Branch name")]
    public string TestBranchName { get; set; } = null!;
        
    [Min(0)]
    [DisplayName("Bench")]
    public int TestBranchBench { get; set; }
    
    [Required]
    [DisplayName("Engine")]
    public int? EngineId { get; set; }
    
    [Required]
    [Min(1)]
    [DisplayName("Number of threads")]
    public int? NumberOfThreads { get; set; }
    
    [Required]
    public int? Priority { get; set; }
    
    [Required]
    public int? HashSize { get; set; }
    
    [Required]
    [RegularExpression("^\\d+\\+\\d+\\.\\d+")]
    public string TimeManagement { get; set; } = null!;
    
    public bool Autobenched { get; set; }
    
    public double? Confidence { get; set; }
    
    [Required]
    [DisplayName("Opening book")]
    public int? OpeningBookId { get; set; }
    
    [Range(0.0, 1.0, MaximumIsExclusive = true, MinimumIsExclusive = true)]
    public double? Alpha { get; set; }
    
    [Range(0.0, 1.0, MaximumIsExclusive = true, MinimumIsExclusive = true)]
    public double? Beta { get; set; }
    
    [Required]
    public double? Elo1 { get; set; }
    
    [Required]
    public double? Elo0 { get; set; }
}



