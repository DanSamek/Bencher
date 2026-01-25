using Shared.Dtos.Responses;
using Worker.TestProcessors;

namespace Worker;

/// <summary>
/// Class that runs worker processes.
/// </summary>
public class Runner
{
    private const int WORKER_AUTOBENCH_NUM_TRIES = 3;
    private const int WAIT_SECONDS = 5;
    
    private readonly RunnerOptions _options;
    private readonly Notifier _notifier;
    private readonly IClientFactory _clientFactory;
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public Runner(RunnerOptions options, Notifier notifier, IClientFactory clientFactory)
    {
        _options = options;
        _notifier = notifier;
        _clientFactory = clientFactory;
    } 

    private async Task Wait()
    {
        ApplicationInfo.Waiting(WAIT_SECONDS);
        await Task.Delay(WAIT_SECONDS * 1000);
    }
    
    public async Task Run(int? nIterations = null)
    {
        var iterations = 0;
        RUN_LOOP:
        while (true)
        {  
            var communication = new Communication(_options, _clientFactory.Get());
            for (var i = 0; i < WORKER_AUTOBENCH_NUM_TRIES && !communication.Error(); i++)
            {
                var autobenchResponse = communication.TryGetAutobenchTest();
                if (autobenchResponse is not null)
                {
                    if (await RunAutobench(communication, autobenchResponse)) continue;
                    goto RUN_LOOP;
                }
                
                StopApplicationIfCommunicationError(communication);
                await Wait();
            }
            
            var testResponse = communication.TryGetTest();
            if (testResponse is not null)
            {
                await RunGames(communication, testResponse);
                StopApplicationIfCommunicationError(communication);
            }
            
            await Wait();
            
            // Avoid missing threads in the ThreadSplitManager.
            iterations++;
            if (nIterations > iterations) return;
        }
    }
    
    private void StopApplicationIfCommunicationError(Communication communication)
    {
        if (!communication.Error()) return;
        
        ApplicationInfo.Display(communication.GetErrorMessage());
        ApplicationInfo.Stopping();
        Environment.Exit(1);
    }
    
    private async Task RunGames(Communication communication, GetTestNonAutobenchResponse testResponse)
    {
        var errorTrace = new ErrorTrace();
        var testProcessor = new GameTestProcessor(communication, errorTrace, testResponse, _options.NumberOfThreads, _notifier);
        var connectionId = testResponse.ConnectionId;
                
        await _notifier.AddNotifyRunningTest(connectionId);
        var result = await testProcessor.Process();
        await _notifier.RemoveNotifyRunningTest(connectionId);
        
        if (result == GameProcessorResult.Error)
        {
            communication.TestError(errorTrace, connectionId);
        }
    }

    private async Task<bool> RunAutobench(Communication communication, GetTestAutobenchResponse autobenchResponse)
    {
        var connectionId = autobenchResponse.ConnectionId;
        var errorTrace = new ErrorTrace();
        var autobenchProcessor = new AutobenchTestProcessor(autobenchResponse, errorTrace, _notifier);
        
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