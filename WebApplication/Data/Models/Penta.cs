using System.ComponentModel.DataAnnotations;

namespace WebApplication.Data;

/// <summary>
/// Pentanomial model [ll, ld, dd, wd, ww].
///     l = lost
///     d = drawn
///     w = win
/// For example ld = lost as white, drawn as black [or reversed]
/// </summary>
public class Penta : DoId
{
    /// <summary>
    /// Pair statistic: [loss, loss].
    /// </summary>
    [Required]
    public int Ll { get; set; } = 0;
    
    /// <summary>
    /// Pair statistic: [loss, draw].
    /// </summary>
    [Required]
    public int Ld { get; set; } = 0;
    
    /// <summary>
    /// Pair statistic: [draw, draw].
    /// </summary>
    [Required]
    public int Dd { get; set; } = 0;
    
    /// <summary>
    /// Pair statistic: [win, draw].
    /// </summary>
    [Required]
    public int Wd { get; set; } = 0;
    
    /// <summary>
    /// Pair statistic: [win, win].
    /// </summary>
    [Required]
    public int Ww { get; set; } = 0;
}