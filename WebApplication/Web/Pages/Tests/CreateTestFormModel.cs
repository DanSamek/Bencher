using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Shared.CustomAttributes;
using WebApplication.Data.Models;

namespace WebApplication.Components.Pages.Tests;

public class CreateTestFormModel
{
    [Required(ErrorMessage = "Test name is required")]
    [DisplayName("Test name")]
    [MaxLength(Test.NAME_MAX_LENGTH)]
    public string TestName { get; set; } = null!;
    
    [MaxLength(Test.DESCRIPTION_MAX_LENGTH, ErrorMessage = "Description is too long")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Base branch name is required")]
    [DisplayName("Branch name")]
    public string BaseBranchName { get; set; } = null!;
        
    [Min(0, ErrorMessage = "Base branch bench has to be > 0")]
    [Required(ErrorMessage = "Base branch bench is required")]
    [DisplayName("Bench")]
    public int? BaseBranchBench { get; set; }
    
    [Required(ErrorMessage = "Test branch name is required")]
    [DisplayName("Branch name")]
    public string TestBranchName { get; set; } = null!;
    
    public const string TEST_BRANCH_BENCH_REQUIRED_ERROR_MESSAGE = "Test branch bench is required";
    [Min(0, ErrorMessage = "Test branch bench has to be > 0")]
    [Required(ErrorMessage = TEST_BRANCH_BENCH_REQUIRED_ERROR_MESSAGE)]
    [DisplayName("Bench")]
    public int? TestBranchBench { get; set; }
    
    [Required(ErrorMessage = "Engine has to be selected")]
    [DisplayName("Engine")]
    public int? EngineId { get; set; }
    
    [Required(ErrorMessage = "Number of threads is required")]
    [Min(1, ErrorMessage = "Invalid number of threads")]
    [DisplayName("Number of threads")]
    public int? NumberOfThreads { get; set; }
    
    [Required(ErrorMessage = "Priority is required")]
    public int? Priority { get; set; }
    
    [Required(ErrorMessage = "Hash size is required")]
    public int? HashSize { get; set; }
    
    [Required(ErrorMessage = "Time management is required")]
    [MaxLength(Test.TM_MAX_LENGTH, ErrorMessage = "Time management value is too big" )]
    [RegularExpression("^\\d+\\+\\d+\\.\\d+", ErrorMessage = "Invalid format for time management, expected for example: 10+0.5")]
    public string TimeManagement { get; set; } = null!;
    
    public bool Autobenched { get; set; }
    
    public double? Confidence { get; set; }
    
    [Required(ErrorMessage = "Opening book is required")]
    [DisplayName("Opening book")]
    public int? OpeningBookId { get; set; }
    
    [Range(0.0, 1.0, MaximumIsExclusive = true, MinimumIsExclusive = true, ErrorMessage = "Invalid value, range: (0,1)")]
    [Required(ErrorMessage = "Alpha is required")]
    public double? Alpha { get; set; }
    
    [Range(0.0, 1.0, MaximumIsExclusive = true, MinimumIsExclusive = true, ErrorMessage = "Invalid value, range: (0,1)")]
    [Required(ErrorMessage = "Beta is required")]
    public double? Beta { get; set; }
    
    [Required(ErrorMessage = "Elo1 is required")]
    public double? Elo1 { get; set; }
    
    [Required(ErrorMessage = "Elo0 is required")]
    public double? Elo0 { get; set; }
    
    [Required(ErrorMessage = "NPS is required")]
    [Min(1, ErrorMessage = "NPS has to be >= 1")]
    public int ExpectedNps { get; set; }
    
    [MaxLength(Test.FASTCHESS_OPTIONS_MAX_LENGTH, ErrorMessage = "Input value is too big")]
    [DisplayName("Additional fast chess options")]
    public string? AdditionalFastchessOptions { get; set; }
}