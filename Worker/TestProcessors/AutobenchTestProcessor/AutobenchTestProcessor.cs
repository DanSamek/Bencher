using Shared.Dtos.Responses;
using Worker.ProcessOperations;
using Worker.UI;

namespace Worker.TestProcessors.AutobenchTestProcessor;

/// <summary>
/// Autobench test processor
/// - Runs bench for the engine
/// </summary>
public class AutobenchTestProcessor : ITestProcessor<int>
{
    private readonly GetTestAutobenchResponse _autobenchResponse;
    private readonly ErrorTrace _errorTrace;
    private readonly Notifier.Notifier _notifier;
    private readonly CommonProcesses _commonProcesses;
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public AutobenchTestProcessor(GetTestAutobenchResponse autobenchResponse, ErrorTrace errorTrace, Notifier.Notifier notifier, CommonProcesses commonProcesses)
    {
        _autobenchResponse = autobenchResponse;
        _errorTrace = errorTrace;
        _notifier = notifier;
        _commonProcesses = commonProcesses;
    }
    
    /// <summary>
    /// Processes autobench.
    /// </summary>
    public Task<int> Process()
    {
        _errorTrace.AddInfo("Processing autobench");
    
        var directory = Directory.CreateTempSubdirectory();
        var connectionId = _autobenchResponse.ConnectionId;
    
        _errorTrace.AddInfo($"Cloning repository - {_autobenchResponse.GitUrl} - branch {_autobenchResponse.TestBranch}");
        _commonProcesses.CloneRepository(_autobenchResponse.GitUrl, _autobenchResponse.TestBranch, 
            directory.FullName, _errorTrace);
        if (_errorTrace.Error() || !_notifier.IsTestStillRunning(connectionId)) return Task.FromResult(0);
    
        _errorTrace.AddInfo("Building engine");
        _commonProcesses.Build(_autobenchResponse.BuildScript!, directory.FullName, _errorTrace);
        if (_errorTrace.Error() || !_notifier.IsTestStillRunning(connectionId)) return Task.FromResult(0);
    
        _errorTrace.AddInfo("Running autobench");
        var (bench, _) = _commonProcesses.RunBench(directory.FullName, _errorTrace);
        Directory.Delete(directory.FullName, true);
        return Task.FromResult(bench);
    }
}