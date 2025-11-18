using WebApplication;

namespace Worker;

/// <summary>
/// Implementation of the authorization with the web app.
/// </summary>
public static class AppAuthorization
{
    /// <summary>
    /// Result of the <see cref="TryLogin" />
    /// </summary>
    /// <param name="Success">If login was successful</param>
    /// <param name="Username">Username of the logged user</param>
    /// <param name="ConnectionId">Connection id - for games</param>
    internal record LoginResult(bool Success, string Username, int ConnectionId);
    
    // TODO move useful constants into separate project.
    /// <summary>
    /// Performs a try to login into web application worker-api.
    /// </summary>
    internal static async Task<LoginResult> TryLogin(string userToken, string webApplicationUrl)
    {
        var loginResult = new LoginResult(false, "", -1);
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add(Shared.WORKER_REQUEST_HEADER, userToken);
            // TODO: The SSL connection could not be established, see inner exception.
            var result = await client.PostAsync($"{webApplicationUrl}/{Shared.WORKER_API_PREFIX}/validate", new StringContent(""));
        }
        catch
        {
            // ignored
        }
        return loginResult;
    }
}