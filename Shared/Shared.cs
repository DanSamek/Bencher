namespace Shared;

public static class Constants
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
    /// Folder, where bencher worker expects engine binary.
    /// </summary>
    public const string BENCHER_BINARY_FOLDER = "bencher_bin";
    
    /// <summary>
    /// Expected filename of the engine binary in the <see cref="BENCHER_BINARY_FOLDER"/>.
    /// </summary>
    public const string BENCHER_BINARY_NAME = "engine";
    
    #region MaybeEnableAppSettingsJson
    
    /// <summary>
    /// Max. size of the log from the worker. 
    /// </summary>
    public const int MAX_LOG_FILE_SIZE = 2097152;
    
    /// <summary>
    /// Number of games, that will be played per thread on the worker.
    /// </summary>
    public const int GAME_THREAD_COUNT_MULTIPLIER = 16;

    #endregion
    
}