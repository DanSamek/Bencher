using System.Text;
using System.Text.RegularExpressions;
using Shared;
using Shared.Dtos.Responses;

namespace Worker;

// TODO what about AVX, SSE, flags when compiling ?
/// <summary>
/// Class that runs worker processes.
/// </summary>
public class Runner
{
    private const int WORKER_AUTOBENCH_NUM_TRIES = 3;
    private const int WAIT_SECONDS = 5;
    private const string BENCH_ENGINE_COMMAND = "bench"; /* TODO add that to the docs */
    
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
            for (var i = 0; i < WORKER_AUTOBENCH_NUM_TRIES; i++)
            {
                var autobenchResponse = await communication.TryGetAutobenchTest();
                if (autobenchResponse is not null)
                {
                    var result = await communication.RunningTest(autobenchResponse.ConnectionId);
                    if (result is null || !result.Running) continue;

                    var autobenchProcess = new AutobenchProcess(autobenchResponse, new ErrorTrace());
                    var connectionId = autobenchResponse.ConnectionId;
                    
                    await _notifier.AddNotifyRunningTest(connectionId);
                    var autobench = ProcessAutobench(autobenchProcess);
                    
                    await _notifier.RemoveNotifyRunningTest(connectionId);

                    if (autobenchProcess.ErrorTrace.Error())
                    {
                        await communication.TestError(autobenchProcess.ErrorTrace, connectionId);
                    }
                    else
                    {
                        await communication.SendAutobenchResult(autobench,connectionId);   
                    }
                    
                    goto RUN_LOOP;
                }

                if (communication.Error)
                {
                    ApplicationInfo.Display(communication.GetErrorMessage());
                    ApplicationInfo.Stopping();
                    return;
                }
                
                await Wait();
            }
            
            var testResponse = await communication.TryGetTest();
            if (testResponse is not null)
            {
                await ProcessTest(testResponse);
            }
            
            await Wait();
        }
    }
    
    
    /// <summary>
    /// Processes autobench:
    /// - Creates directory in the /tmp
    /// - Clones a repository + branch checkout
    /// - Builds the engine
    /// - Runs "bench" command
    /// </summary>
    private int ProcessAutobench(AutobenchProcess autobenchProcess)
    {
        var autobenchResponse = autobenchProcess.AutobenchResponse;
        var errorTrace = autobenchProcess.ErrorTrace;
        errorTrace.AddInfo("Processing autobench");
        
        var directory = Directory.CreateTempSubdirectory();
        
        errorTrace.AddInfo($"Cloning repository - {autobenchResponse.GitUrl} - branch {autobenchResponse.TestBranch}");
        CloneRepository(autobenchResponse.GitUrl, autobenchResponse.TestBranch, 
            directory.FullName, autobenchProcess.ErrorTrace);
        if (errorTrace.Error()) return 0;
        
        errorTrace.AddInfo("Building engine");
        Build(autobenchResponse.BuildScript!, directory.FullName, errorTrace);
        if (errorTrace.Error()) return 0;
        
        errorTrace.AddInfo("Running autobench");
        var (bench, _) = RunBench(directory.FullName, errorTrace);
        Directory.Delete(directory.FullName, true);
        return bench;
    }

    private void Build(byte[] buildScript, string directoryFullName, ErrorTrace errorTrace)
    {
        var script = $"cd {directoryFullName}; {Encoding.ASCII.GetString(buildScript)}";
        var processInfo = Helper.CreateProcessStartInfo(script);
        var (output, error) = Helper.RunProcess(processInfo);
        
        errorTrace.AddInfoError(output, error);
    }
    
    private void CloneRepository(string gitUrl, string testBranch, string directoryPath, ErrorTrace trace)
    {
        var cloneScript = $"cd {directoryPath}; git clone {gitUrl} .; git checkout {testBranch}";
        var processInfo = Helper.CreateProcessStartInfo(cloneScript);
        var (output, error) = Helper.RunProcess(processInfo);
        trace.AddInfo(output);
        
        // error:
        var errors = Regexes.GitErrorRegex.Match(error ?? "");
        foreach (Group group in errors.Groups)  
        {
            trace.AddError(group.Value);   
        }
    }

    private record BenchResult(int Bench, int Nps);
    
    private BenchResult RunBench(string directoryFullName, ErrorTrace errorTrace)
    {
        var binaryPath = $"{directoryFullName}/{Constants.BENCHER_BINARY_FOLDER}/{Constants.BENCHER_BINARY_NAME}";
        var info = Helper.CreateProcessStartInfo(BENCH_ENGINE_COMMAND, binaryPath); 
        var (output, _) = Helper.RunProcess(info);
        errorTrace.AddInfo(output);
        
        if (output is null) return new BenchResult(0, 0);
        
        // Try find bench value in the output 
        var match = Regexes.BenchRegex.Match(output);
        if (!match.Success)
        {
            errorTrace.AddError($"Unable to parse bench and nps, expected format: \"bench: 6479310, nps: 3896157\"");
            return new BenchResult(0, 0);
        }
        
        var bench = int.Parse(match.Groups[1].Value);
        var nps = int.Parse(match.Groups[2].Value);
        var result = new BenchResult(bench, nps);
        return result;
    }
    
    private async Task ProcessTest(GetTestNonAutobenchResponse testResponse)
    {
        await Task.Delay(0);
    }
}

internal record AutobenchProcess(GetTestAutobenchResponse AutobenchResponse, ErrorTrace ErrorTrace);