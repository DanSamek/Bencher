using Worker.Dependencies;

namespace Worker;

public static class Program
{
    public static async Task Main(string[] args)
    {
        ApplicationInfo.Display();
        var runnerOptions = new RunnerOptions();
        var logged = false;
        while (!logged)
        {
            var (success, webApplicationUrl, userToken) = LoginParamsLoader.LoadLoginParams();
            if (!success) continue;
            var result = await Communication.TryLogin(userToken, webApplicationUrl);
            logged = result.Success;
            if (result.Success) ApplicationInfo.ShowLoggedUser(result.Username);
            
            runnerOptions.UserToken = userToken;
            runnerOptions.WebApplicationUrl = webApplicationUrl;
        }
        
        var userOptions = UserOptionsLoader.LoadParams();
        var (compilers, trace) = DependencyValidator.Validate();
        if (trace.Error())
        {
            await StopApplicationAndSendMessage(runnerOptions, trace);
            return;
        }
        
        trace = DependencyResolver.TryResolve(compilers);
        if (trace.Error())
        {
            await StopApplicationAndSendMessage(runnerOptions, trace);
            return;
        }
        
        var notifier = new Notifier(runnerOptions);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(() => notifier.Run());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        
        if (userOptions.TrySplitThreads)
        {
            // TODO run n times runner
        }
        else
        {
            runnerOptions.NumberOfThreads = userOptions.NumberOfThreads;
            var runner = new Runner(runnerOptions, notifier);
            await runner.Run();   
        }
    }

    private static async Task StopApplicationAndSendMessage(RunnerOptions runnerOptions, ErrorTrace trace)
    {
        var communication = new Communication(runnerOptions);
        await communication.WorkerError(trace);
        Console.WriteLine(trace);
        ApplicationInfo.Stopping();
    }
}

