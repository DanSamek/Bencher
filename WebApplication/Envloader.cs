namespace WebApplication;

/// <summary>
/// Helper class for the environment.
/// </summary>
public static class Envloader
{   
    private const string ENV_DB_HOST = "DB_HOST";
    private const string ENV_DB_NAME = "DB_NAME";
    private const string ENV_DB_USER = "DB_USER";
    private const string ENV_DB_PASS = "DB_PASS";
    
    /// <summary>
    /// Loads connection string from the environment.
    /// </summary>
    public static string GetConnectionString()
    {
        var dbHost = Environment.GetEnvironmentVariable(ENV_DB_HOST);
        var dbName = Environment.GetEnvironmentVariable(ENV_DB_NAME);
        var dbUser = Environment.GetEnvironmentVariable(ENV_DB_USER);
        var dbPassword = Environment.GetEnvironmentVariable(ENV_DB_PASS);
        
        if (dbHost == null || dbName == null || dbUser == null || dbPassword == null)
        {
            throw new Exception("Database environment variables are not set.");
        }
        
        var connectionString = $"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPassword}";
        return connectionString;
    }
}