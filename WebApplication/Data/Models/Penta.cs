using System.ComponentModel.DataAnnotations;

namespace WebApplication.Data.Models;

/// <summary>
/// Pentanomial statistics [ll, ld, dd/wl, wd, ww].
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
    public int Ll { get; set; }
    
    /// <summary>
    /// Pair statistic: [loss, draw].
    /// </summary>
    [Required]
    public int Ld { get; set; }
    
    /// <summary>
    /// Pair statistic: [draw, draw]
    /// </summary>
    [Required]
    public int Dd { get; set; }

    /// <summary>
    /// Pair statistic: [win, lose]
    /// </summary>
    [Required]
    public int Wl { get; set; }
    
    /// <summary>
    /// Pair statistic: [win, draw].
    /// </summary>
    [Required]
    public int Wd { get; set; }
    
    /// <summary>
    /// Pair statistic: [win, win].
    /// </summary>
    [Required]
    public int Ww { get; set; }
    
    /// <summary>
    /// Test that belongs to this penta. 
    /// </summary>
    [Required]
    public required Test Test { get; set; }
    
    /// <summary>
    /// Id of the test.
    /// </summary>
    public int TestId { get; set; }

    private int DdWl => Dd + Wl;
    
    /// <summary>
    /// WDL statistics
    /// </summary>
    /// <param name="W">Win</param>
    /// <param name="D">Lose</param>
    /// <param name="L">Draw</param>
    public record Wdl(int W, int D, int L);
    
    /// <summary>
    /// Simplified penta - [dd + wl] as one property. 
    /// </summary>
    public record RawPentanomial(int Ll, int Ld, int Dd, int Wd, int Ww);
    
    public Wdl ToWdl()
    {
        var wins = Ww * 2 + Wd + Wl;
        var loses = Ll * 2 + Wl + Ld;
        var draws = Dd * 2 + Ld + Wd;
        
        var result = new Wdl(wins, draws, loses);
        return result;
    }

    public RawPentanomial ToRawPentanomial() => new RawPentanomial(Ll, Ld, DdWl, Wd, Ww);
    
    private int TotalPairs => Ll + Ld + DdWl + Wd + Wl;
    
    // W = 0.5
    // D = 0.25
    // L = 0
    private double TotalPoints => Ll * 0 + Ld * 0.25 + DdWl * 0.5 + Wd * 0.75 + Ww * 1;
    public double Score => TotalPoints / TotalPairs;
}
