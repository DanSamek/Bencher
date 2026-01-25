using Worker.Dependencies;

namespace Worker;

public static class Program
{
    public static async Task Main(string[] args)
    {
        ApplicationInfo.Display();
        var runnerOptions = new RunnerOptions();
        
        var logged = false;
        var factory = new ClientFactory(runnerOptions);
        while (!logged)
        {
            var (success, webApplicationUrl, userToken) = LoginParamsLoader.LoadLoginParams();
            if (!success) continue;
            var result = await Login.TryLogin(userToken, webApplicationUrl, new HttpClient());
            logged = result.Success;
            if (result.Success) ApplicationInfo.ShowLoggedUser(result.Username);

            runnerOptions.UserToken = userToken;
            runnerOptions.WebApplicationUrl = webApplicationUrl;
        }

        var userOptions = UserOptionsLoader.LoadParams();
        var (compilers, trace) = DependencyValidator.Validate();
        if (trace.Error())
        {
            StopApplicationAndSendMessage(runnerOptions, trace, factory);
            return;
        }

        trace = DependencyResolver.TryResolve(compilers);
        if (trace.Error())
        {
            StopApplicationAndSendMessage(runnerOptions, trace, factory);
            return;
        }

        var notifier = new Notifier(new Communication(runnerOptions, factory.Get()));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(() => notifier.Run());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        
        runnerOptions.NumberOfThreads = userOptions.NumberOfThreads;
        if (userOptions.TrySplitThreads)
        {
            var threadSplit = new ThreadSplitManager.ThreadSplitManager(runnerOptions, notifier, new Communication(runnerOptions, factory.Get()), factory);
            threadSplit.Run();
        }
        else
        {
            var runner = new Runner(runnerOptions, notifier, factory);
            await runner.Run();
        }
    }

    private static void StopApplicationAndSendMessage(RunnerOptions runnerOptions, ErrorTrace trace, IClientFactory factory)
    {
        var communication = new Communication(runnerOptions, factory.Get());
        communication.WorkerError(trace);
        Console.WriteLine(trace);
        ApplicationInfo.Stopping();
    }
}

