namespace Worker;

internal static class LoginParamsLoader
{
    internal record LoginParams(bool Success, string WebApplicationUrl, string UserToken);
    
    /// <summary>
    /// Loads user's login params.
    /// </summary>
    internal static LoginParams LoadLoginParams()
    {
        Console.WriteLine("Web application url: ");
        var url = Console.ReadLine();
        Console.WriteLine("Access token: ");
        var userToken = Console.ReadLine();
        
        var result = new LoginParams(url is not null && userToken is not null, url ?? string.Empty, userToken ?? string.Empty);
        return result;
    }
}