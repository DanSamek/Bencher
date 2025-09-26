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
}