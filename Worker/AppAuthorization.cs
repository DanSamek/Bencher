using Newtonsoft.Json;
using Shared;
using Shared.Dtos.Responses;

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
    internal record LoginResult(bool Success, string Username);

    private static LoginResult INVALID_LOGIN = new LoginResult(false, string.Empty);
    
    /// <summary>
    /// Performs a try to login into web application worker-api.
    /// </summary>
    internal static async Task<LoginResult> TryLogin(string userToken, string webApplicationUrl)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add(Constants.WORKER_REQUEST_HEADER, userToken);
            // The SSL connection could not be established, see inner exception --> dotnet dev-certs https --trust
            var response = await client.PostAsync($"{webApplicationUrl}/{Constants.WORKER_API_PREFIX}/validate", new StringContent(""));

            using var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync());
            using var jsonTextReader = new JsonTextReader(streamReader);
            var serializer = new JsonSerializer();
            var result = serializer.Deserialize<ValidateResponseDto>(jsonTextReader);
            
            var loginResult = result?.Username is null
                ? INVALID_LOGIN
                : new LoginResult(Username: result.Username, Success: true);
            return loginResult;
        }
        catch
        {
            // ignored TODO log.
        }
        return INVALID_LOGIN;
    }
}