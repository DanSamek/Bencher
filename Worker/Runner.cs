using Shared.Dtos.Responses;
using Worker.TestProcessors;

namespace Worker;

// TODO what about AVX, SSE, flags when compiling [technically this can be done by user build script.]
/// <summary>
/// Class that runs worker processes.
/// </summary>
public class Runner
{
    private const int WORKER_AUTOBENCH_NUM_TRIES = 3;
    private const int WAIT_SECONDS = 5;
    
    private readonly RunnerOptions _options;
    private readonly Notifier _notifier;
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public Runner(RunnerOptions options, Notifier notifier)
    {
        _options = options;
        _notifier = notifier;
    } 

    private async Task Wait()
    {
        ApplicationInfo.Waiting(WAIT_SECONDS);
        await Task.Delay(WAIT_SECONDS * 1000);
    }
    
    public async Task Run()
    {
        RUN_LOOP:
        while (true)
        {  
            var communication = new Communication(_options); // TODO somehow refresh httpclients.
            for (var i = 0; i < WORKER_AUTOBENCH_NUM_TRIES && !communication.Error; i++)
            {
                var autobenchResponse = communication.TryGetAutobenchTest();
                if (autobenchResponse is not null)
                {
                    if (await RunAutobench(communication, autobenchResponse)) continue;
                    goto RUN_LOOP;
                }

                // TODO dry for game test.
                if (communication.Error)
                {
                    ApplicationInfo.Display(communication.GetErrorMessage());
                    ApplicationInfo.Stopping();
                    return;
                }
                
                await Wait();
            }
            
            var testResponse = communication.TryGetTest();
            if (testResponse is not null)
            {
                var errorTrace = new ErrorTrace();
                var testProcessor = new GameTestProcessor(communication, errorTrace, testResponse, _options.NumberOfThreads);
                var connectionId = testResponse.ConnectionId;
                
                await _notifier.AddNotifyRunningTest(connectionId);
                    var result = await testProcessor.Process();
                await _notifier.RemoveNotifyRunningTest(connectionId);
                
                // TODO
            }
            
            await Wait();
        }
    }

    private async Task<bool> RunAutobench(Communication communication, GetTestAutobenchResponse autobenchResponse)
    {
        var result = communication.RunningTest(autobenchResponse.ConnectionId);
        if (result is null || !result.Running) return true;

        var errorTrace = new ErrorTrace();
        var connectionId = autobenchResponse.ConnectionId;
        var autobenchProcessor = new AutobenchTestProcessor(autobenchResponse, errorTrace);
        
        await _notifier.AddNotifyRunningTest(connectionId);
            var autobench = await autobenchProcessor.Process();
        await _notifier.RemoveNotifyRunningTest(connectionId);

        if (errorTrace.Error())
        {
            communication.TestError(errorTrace, connectionId);
        }
        else
        {
            communication.SendAutobenchResult(autobench,connectionId);   
        }

        return false;
    }
}