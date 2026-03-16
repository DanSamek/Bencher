
using WebApplication.Data.Models;

namespace WebApplication.SPRT;

/// <summary>
/// https://en.wikipedia.org/wiki/Sequential_probability_ratio_test
/// https://cantate.be/Fishtest/normalized_elo_practical.pdf
/// </summary>
public static class Sprt
{
    public enum  SprtResult
    {
        H0NotRejected, // Null 
        H0Rejected,    // Alternative
        Unknown        // Still needs to run
    }
    
    /*
     * Inversion of the: https://cran.r-project.org/web/packages/elo/vignettes/intro.html
     * EloDiff = -(400 * (log (1 / P_a - 1))) / log(10).
     */
    private static double ScoreToElo(double score)
        => -(400 * Math.Log(1.0 / score - 1)) / Math.Log(10);
    
    /*
     * Formula is in the paper: https://cantate.be/Fishtest/normalized_elo_practical.pdf
     */
    private static double NeloToScore(double nElo, double stdDeviation) 
        => nElo * Math.Sqrt(2) * stdDeviation / (800 / Math.Log(10)) + 0.5;
    
    
    /*
     * 95% CI
     * Inversion of the standard distribution function for alpha = 2.5. (alpha quantile).
     */
    private const double ZAlpha2_5 = 1.96;
    
    /// <summary>
    /// All SPRT statistics, that render on the page. 
    /// </summary>
    public record Statistics
    {
        public required SprtResult Result { get; set; }
        public required double Llr { get; set; }
        public required double Elo { get; set; }
        
        public required double EloUpperBound { get; set; }
        
        public required double EloLowerBound { get; set; }
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
        var result = new Statistics
        {
            Elo = 0,
            Result = SprtResult.Unknown,
            Wdl = test.Penta.ToWdl(),
            RawPentanomial = test.Penta.ToRawPentanomial(),
            Bounds = test.Settings.GetErrorBounds(),
            EloUpperBound = 0,
            EloLowerBound = 0,
            Llr = 0,
        };
        
        var penta = test.Penta;
        var totalPairs = (double)penta.TotalPairs();
        if (totalPairs <= 1) return result;
        
        var score = penta.Score();
        var variance = Variance(penta, score);
        var deviation = Math.Sqrt(variance);
        
        var scoreElo0 = NeloToScore(test.Settings.Elo0, deviation);
        var scoreElo1 = NeloToScore(test.Settings.Elo1, deviation);
        
        var varianceElo0 = Variance(penta, scoreElo0);
        var varianceElo1 = Variance(penta, scoreElo1);
        
        var llr = totalPairs / 2 * Math.Log(varianceElo0 / varianceElo1);
        
        // Confidence interval 95%
        var scoreLowerBound = score - ZAlpha2_5 * deviation / Math.Sqrt(totalPairs);
        var scoreUpperBound = score + ZAlpha2_5 * deviation / Math.Sqrt(totalPairs);

        var elo = ScoreToElo(score);
        var eloLowerBound = elo - ScoreToElo(scoreLowerBound);
        var eloUpperBound = ScoreToElo(scoreUpperBound) - elo;
        
        result = result with
        {
            Elo = elo,
            EloLowerBound = eloLowerBound,
            EloUpperBound = eloUpperBound,
            Llr = llr,
            Result = llr > result.Bounds.Type2ErrorBound ? SprtResult.H0Rejected :
                     llr < result.Bounds.Type1ErrorBound ? SprtResult.H0NotRejected : 
                     SprtResult.Unknown,
        };
        
        return result;
    }
    
    private static double Variance(Penta penta, double score)
    {
        var totalPairs = penta.TotalPairs(); 
        
        var llVariance = Elem(penta.Ll, 0, score);
        var ldVariance = Elem(penta.Ld, 0.25, score);
        var wlVariance = Elem(penta.DdWl, 0.5, score);
        var wdVariance = Elem(penta.Wd, 0.75, score);
        var wwVariance = Elem(penta.Ww, 1, score);
        
        var variance = llVariance + ldVariance + wlVariance + wdVariance + wwVariance;
        variance /= totalPairs - 1;
        return variance;
    }
    
    private static double Elem(int m, double xi, double xn)
        => m * Math.Pow(xi - xn, 2);
}