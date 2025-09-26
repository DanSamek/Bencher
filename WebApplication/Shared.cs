namespace WebApplication;

public static class Shared
{
    /// <summary>
    /// Users access token header key in the request.
    /// </summary>
    public const string WORKER_REQUEST_HEADER = "AccessToken";
    
    /// <summary>
    /// Prefix, that will be used for workers to communicate with the web application.
    /// </summary>
    public const string WORKER_API_PREFIX = "worker-api";
    
    /// <summary>
    /// Max. size of the log from the worker. 
    /// </summary>
    public const int MAX_LOG_FILE_SIZE = 2097152;
}