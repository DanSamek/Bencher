namespace Worker;

public record RunnerOptions
{
    /// <summary>
    /// Web application url for communication.
    /// </summary>
    public string WebApplicationUrl { get; set; } = null!;
 
    /// <summary>
    /// Number of threads that will be used by program [to run fastchess/cutechess + communication]
    /// </summary>
    public int NumberOfThreads { get; set; }

    /// <summary>
    /// Users token for authorization.
    /// </summary>
    public string UserToken { get; set; } = null!;
}