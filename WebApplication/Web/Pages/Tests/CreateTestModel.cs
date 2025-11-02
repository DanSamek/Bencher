using System.ComponentModel.DataAnnotations;

namespace WebApplication.Components.Pages.Tests;

internal class CreateTestModel
{
    [Required]
    public string Description { get; set; } = null!;

    public Branch TestBranch { get; set; } = new();
    public Branch BaseBranch { get; set; } = new();
        
    [Required]
    public SPRTSettings? SprtSettings { get; set; }
        
    public TestSettings TestSettings { get; set; } = new();
}

internal class Branch
{
    [Required] 
    public string Name { get; set; } = null!;
        
    // TODO we will call git api to get bench.
    [Required]
    public int Bench { get; set; }
}

internal class SPRTSettings
{
    [Required]
    public int Elo0 { get; set; }
    
    [Required]
    public int Elo1 { get; set; }
    
    [Required]
    public double Alpha { get; set; }
    
    [Required]
    public double Beta { get; set; }
}

internal class TestSettings
{
    [Required]
    public bool Autobenched { get; set; }
        
    [Required]
    public int Priority { get; set; }
        
    [Required]
    public int NumberOfThreads { get; set; }
        
    [Required]
    public int HashSize { get; set; }

    [Required]
    [MaxLength(30)]
    [RegularExpression("\\d+\\+\\d\\.\\d+")] // 8+0.08
    public string TimeManagement { get; set; } = null!;
}