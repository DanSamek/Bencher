namespace Worker;

internal static class LoginParamsLoader
{
    internal record LoginParams(bool Success, string WebApplicationUrl, string UserToken);
    
    /// <summary>
    /// Loads user's login params.
    /// </summary>
    internal static LoginParams LoadLoginParams()
    {
        #if DEBUG
        return new LoginParams(true, "https://localhost:7240", "ZLZCR6KJ81GZXTQA");
        #else
        Console.WriteLine("Web application url: ");
        var url = Console.ReadLine();
        Console.WriteLine("Access token: ");
        var userToken = Console.ReadLine();

        if (url is not null && url[^1] == '/')
        {
            url = url[1..^1];   
        }
        var result = new LoginParams(url is not null && userToken is not null, url ?? string.Empty, userToken ?? string.Empty);
        
        return result;
        #endif
    }
}