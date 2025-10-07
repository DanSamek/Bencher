using System.ComponentModel.DataAnnotations;

namespace WebApplication.Data.Models;

/// <summary>
/// https://en.wikipedia.org/wiki/Sequential_probability_ratio_test
/// </summary>
public class SprtSettings : DoId
{
    /// <summary>
    /// Null hypothesis.
    /// </summary>
    [Required]
    public required int Elo0 { get; set; }
    
    /// <summary>
    /// Alternative hypothesis.
    /// </summary>
    [Required]
    public required int Elo1 { get; set; }
    
    /// <summary>
    /// False positive confidence.
    /// </summary>
    [Required]
    public required double Alpha { get; set; }
    
    /// <summary>
    /// False negative confidence.
    /// </summary>
    [Required]
    public required double Beta { get; set; }
    
    /// <summary>
    /// Tests that belongs to those settings.
    /// TODO rename to the Tests!
    /// </summary>
    [Required]
    public required List<Test> Test { get; set; } = [];
    
    /// <summary>
    /// Test error bounds.
    /// </summary>
    /// <param name="Type1ErrorBound">False positive.</param>
    /// <param name="Type2ErrorBound">False negative.</param> 
    public record Bounds(double Type1ErrorBound, double Type2ErrorBound);
    public Bounds GetErrorBounds() => new Bounds(Math.Log(Beta / (1 - Alpha)), Math.Log((1 - Beta) / Alpha));
}