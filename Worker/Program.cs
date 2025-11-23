namespace Worker;

public static class Program
{
    public static async Task Main(string[] args)
    {
        ApplicationInfo.Display();
        var logged = false;
        while (!logged)
        {
            var (success, webApplicationUrl, userToken) = LoginParamsLoader.LoadLoginParams();
            if (!success) continue;
            var result = await AppAuthorization.TryLogin(userToken, webApplicationUrl);
            logged = result.Success;
        }

        //var userOptions = LoadUserOptions();
    }
}

