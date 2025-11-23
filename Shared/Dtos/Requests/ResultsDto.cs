using System.ComponentModel.DataAnnotations;
using Shared.CustomAttributes;

namespace Shared.Dtos.Requests;

/// <summary>
/// Request body for <see cref="WorkerController.Results" />  
/// </summary>
public class ResultsDto : WithConnectionId
{
    /// <summary>
    /// Pair statistic: [loss, loss].
    /// </summary>
    [Required]
    [Min(0)]
    public required int Ll { get; set; }
    
    /// <summary>
    /// Pair statistic: [loss, draw].
    /// </summary>
    [Required]
    [Min(0)]
    public required int Ld { get; set; }
    
    /// <summary>
    /// Pair statistic: [draw, draw]
    /// </summary>
    [Required]
    [Min(0)]
    public required int Dd { get; set; }
    
    /// <summary>
    /// Pair statistic: [win, lose]
    /// </summary>
    [Required]
    [Min(0)]
    public required int Wl { get; set; }
    
    /// <summary>
    /// Pair statistic: [win, draw].
    /// </summary>
    [Required]
    [Min(0)]
    public required int Wd { get; set; }
    
    /// <summary>
    /// Pair statistic: [win, win].
    /// </summary>
    [Required]
    [Min(0)]
    public required int Ww { get; set; }
}