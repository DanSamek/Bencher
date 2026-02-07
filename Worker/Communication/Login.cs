using Shared;
using Shared.Dtos.Responses;
using Worker.UI;

namespace Worker;

public class Login
{
    /// <summary>
    /// Result of the <see cref="TryLogin" />
    /// </summary>
    /// <param name="Success">If login was successful</param>
    /// <param name="Username">Username of the logged user</param>
    public record LoginResult(bool Success, string Username);
    
    private static LoginResult INVALID_LOGIN = new (false, string.Empty);
    
    public static async Task<LoginResult> TryLogin(string userToken, string webApplicationUrl, HttpClient client)
    {
        // TODO docs for development: The SSL connection could not be established, see inner exception --> dotnet dev-certs https --trust
        try
        {
            client.DefaultRequestHeaders.Add(Constants.WORKER_REQUEST_HEADER, userToken);
            var response = await client.PostAsync($"{webApplicationUrl}/{Constants.WORKER_API_PREFIX}/validate", new StringContent(""));

            var result = Helper.Deserialize<ValidateResponseDto>(response);
            var loginResult = result?.Username is null
                ? INVALID_LOGIN
                : new LoginResult(Username: result.Username, Success: true);
            return loginResult;
        }
        catch
        {
            ApplicationInfo.ShowUnableToLogin(webApplicationUrl);
        }
        return INVALID_LOGIN;
    }

}