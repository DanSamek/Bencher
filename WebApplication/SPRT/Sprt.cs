
using WebApplication.Data.Models;

namespace WebApplication.SPRT;

/// <summary>
/// https://en.wikipedia.org/wiki/Sequential_probability_ratio_test
/// </summary>
public static class Sprt
{
    public enum  SprtResult
    {
        H0Accepted, // Null 
        H1Accepted, // Alternative
        Unknown     // Still needs to run
    }
    
    /// <summary>
    /// All SPRT statistics, that we will render on the page. 
    /// </summary>
    public record Statistics
    {
        public SprtResult Result { get; set; }
        public double LOS { get; set; }
        public double Llr { get; set; }
        public double Elo { get; set; }
        
        public required Penta.Wdl Wdl { get; set; }
        public required Penta.RawPentanomial RawPentanomial { get; set; }
        public required SprtSettings.Bounds Bounds { get; set; }
    }
    
    /// <summary>
    /// Calculates test statistics for a test.
    /// Requirements: Included Penta.
    /// Note, we don't store statistics in the database so far.
    /// </summary>
    public static Statistics GetStatistics(Test test)
    {
        var score = test.Penta.Score;
        // TODO !!
        var result = new Statistics
        {
            Result =  SprtResult.Unknown,
            Wdl = test.Penta.ToWdl(),
            RawPentanomial = test.Penta.ToRawPentanomial(),
            Bounds = test.Settings.GetErrorBounds()
        };
        return result;
    }
}