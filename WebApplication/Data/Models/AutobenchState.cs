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
    public bool Resolved =>  1.0 - Confidence <= 0.0001; // For autobench purposes is okay. We don't expect that user want to run > 1 / 0.0001 autobenches.

    /// <summary>
    /// Confidence of the user - it basically means, what is a chance, that this code doesn't have UB.
    /// Will be used in the calculation of the <see cref="Confidence" />.
    /// Note: Valid values are (0.0001,1].
    /// </summary>
    [Required]
    public required double UserConfidence { get; set; } = 1; // 100%

    /// <summary>
    /// Updates <see cref="Confidence"/>, if bench is the same.
    /// If not, false is returned and test should be stopped.
    /// </summary>
    public bool UpdateConfidence(int workerBench)
    {
        if (workerBench != Bench) return false;
        
        Confidence += UserConfidence;
        return true;
    }
}