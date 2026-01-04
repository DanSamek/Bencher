using Shared.Dtos.Responses;

namespace Worker.TestProcessors;
public record AutobenchProcess(GetTestAutobenchResponse AutobenchResponse, ErrorTrace ErrorTrace);

/// <summary>
/// Implementation of the autobench test "pipeline".
/// </summary>
public class AutobenchTestProcessor : ITestProcessor<AutobenchProcess, int>
{
    /// <summary>
    /// Processes autobench:
    /// - Creates directory in the /tmp
    /// - Clones a repository + branch checkout
    /// - Builds the engine
    /// - Runs "bench" command
    /// </summary>
    public Task<int> Process(AutobenchProcess autobenchProcess)
    {
        var autobenchResponse = autobenchProcess.AutobenchResponse;
        var errorTrace = autobenchProcess.ErrorTrace;
        errorTrace.AddInfo("Processing autobench");
    
        var directory = Directory.CreateTempSubdirectory();
    
        errorTrace.AddInfo($"Cloning repository - {autobenchResponse.GitUrl} - branch {autobenchResponse.TestBranch}");
        ProcessorHelper.CloneRepository(autobenchResponse.GitUrl, autobenchResponse.TestBranch, 
            directory.FullName, autobenchProcess.ErrorTrace);
        if (errorTrace.Error()) return Task.FromResult(0);
    
        errorTrace.AddInfo("Building engine");
        ProcessorHelper.Build(autobenchResponse.BuildScript!, directory.FullName, errorTrace);
        if (errorTrace.Error()) return Task.FromResult(0);
    
        errorTrace.AddInfo("Running autobench");
        var (bench, _) = ProcessorHelper.RunBench(directory.FullName, errorTrace);
        Directory.Delete(directory.FullName, true);
        return Task.FromResult(bench);
    }
}