using Shared.Dtos.Responses;

namespace Worker.TestProcessors;
public record AutobenchProcess(GetTestAutobenchResponse AutobenchResponse, ErrorTrace ErrorTrace);

/// <summary>
/// Implementation of the autobench test "pipeline".
/// </summary>
public class AutobenchTestProcessor : ITestProcessor<int>
{
    private readonly GetTestAutobenchResponse _autobenchResponse;
    private readonly ErrorTrace _errorTrace;
    /// <summary>
    /// .Ctor
    /// </summary>
    public AutobenchTestProcessor(GetTestAutobenchResponse autobenchResponse, ErrorTrace errorTrace)
    {
        _autobenchResponse = autobenchResponse;
        _errorTrace = errorTrace;
    }
    
    /// <summary>
    /// Processes autobench:
    /// - Creates directory in the /tmp
    /// - Clones a repository + branch checkout
    /// - Builds the engine
    /// - Runs "bench" command
    /// </summary>
    public Task<int> Process()
    {
        _errorTrace.AddInfo("Processing autobench");
    
        var directory = Directory.CreateTempSubdirectory();
    
        _errorTrace.AddInfo($"Cloning repository - {_autobenchResponse.GitUrl} - branch {_autobenchResponse.TestBranch}");
        ProcessorHelper.CloneRepository(_autobenchResponse.GitUrl, _autobenchResponse.TestBranch, 
            directory.FullName, _errorTrace);
        if (_errorTrace.Error()) return Task.FromResult(0);
    
        _errorTrace.AddInfo("Building engine");
        ProcessorHelper.Build(_autobenchResponse.BuildScript!, directory.FullName, _errorTrace);
        if (_errorTrace.Error()) return Task.FromResult(0);
    
        _errorTrace.AddInfo("Running autobench");
        var (bench, _) = ProcessorHelper.RunBench(directory.FullName, _errorTrace);
        Directory.Delete(directory.FullName, true);
        return Task.FromResult(bench);
    }
}