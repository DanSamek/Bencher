namespace WebApplication;

/// <summary>
/// Helper class for the environment.
/// </summary>
public static class Envloader
{   
    /// <summary>
    /// Loads connection string from the environment.
    /// </summary>
    public static string GetConnectionString()
    {
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME");
        var dbUser = Environment.GetEnvironmentVariable("DB_USER");
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASS");
        
        if (dbHost == null || dbName == null || dbUser == null || dbPassword == null)
        {
            throw new Exception("Database environment variables not found.");
        }
        
        var connectionString = $"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPassword}";
        return connectionString;
    }
}