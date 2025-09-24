using System.ComponentModel.DataAnnotations;

namespace WebApplication.Data.Models;

/// <summary>
/// State of the autobench.
/// </summary>
public class AutobenchState : DoId
{
    /// <summary>
    /// Test, that is autobenched. 
    /// </summary>
    [Required]
    public required Test Test { get; set; }
    
    /// <summary>
    /// Id of the test.
    /// </summary>
    [Required]
    public required int TestId { get; set; }
    
    /// <summary>
    /// Probably correct bench [this value is taken from first response of the worker].
    /// </summary>
    [Required]
    public int? Bench { get; set; }
    
    /// <summary>
    /// Confidence of the autobench.  
    /// </summary>
    [Required]
    public required double Confidence { get; set; }
    
    /// <summary>
    /// If autobench was resolved. 
    /// </summary>
    [Required]
    public required bool Resolved { get; set; }
    
    /// <summary>
    /// Confidence of the user.
    /// Will be used in the calculation of the <see cref="Confidence" />.
    /// </summary>
    [Required]
    public required double UserConfidence { get; set; }
        
}