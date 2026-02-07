namespace Worker.TestProcessors.GameTestProcessor;
public static class TmScaler
{
    /// <summary>
    /// Time management scaler - for faster/slower workers than expected.
    /// </summary>
    public static (decimal Seconds, decimal Increment) Scale(int baseNps, int expectedNps, string timeManagement)
    {
        var timeScale = expectedNps * (decimal)1.0 / baseNps;
        (decimal seconds, decimal increment) = timeManagement.Tm();
        seconds *= timeScale;
        increment *= timeScale;
        return (seconds, increment);
    }

}