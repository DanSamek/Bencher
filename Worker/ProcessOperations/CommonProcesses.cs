using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Shared;
using Worker.Dependencies;
using Worker.UI;

namespace Worker.ProcessOperations;

public class CommonProcesses
{
    private readonly ProcessRunner _runner;
    private readonly ProcessStartInfoCreator _processInfoCreator;
    
    private const string BENCH_ENGINE_COMMAND = "bench"; /* TODO add that to the docs */

    /// <summary>
    /// .Ctor
    /// </summary>
    public CommonProcesses(ProcessRunner runner, ProcessStartInfoCreator processInfoCreator)
    {
        _runner = runner;
        _processInfoCreator = processInfoCreator;
    }
    
    public void Build(byte[] buildScript, string directoryFullName, ErrorTrace errorTrace)
    {
        var script = $"cd {directoryFullName}; {Encoding.ASCII.GetString(buildScript)}";
        var processInfo = _processInfoCreator.Create(script);
        var (output, error) = _runner.RunProcess(processInfo);
        
        errorTrace.AddInfoError(output, error);
    }
    
    public void CloneRepository(string gitUrl, string testBranch, string directoryPath, ErrorTrace trace)
    {
        var cloneScript = $"cd {directoryPath}; git clone {gitUrl} .; git checkout {testBranch}";
        var processInfo = _processInfoCreator.Create(cloneScript);
        var (output, error) = _runner.RunProcess(processInfo);
        trace.AddInfo(output);
        
        // error:
        var errors = Regexes.GitErrorRegex.Match(error ?? "");
        foreach (Group group in errors.Groups)  
        {
            trace.AddError(group.Value);   
        }
    }
    
    public record BenchResult(int Bench, int Nps);
    
    public BenchResult RunBench(string directoryFullName, ErrorTrace errorTrace)
    {
        var binaryPath = $"{directoryFullName}/{Constants.BENCHER_BINARY_FOLDER}/{Constants.BENCHER_BINARY_NAME}";
        var info = _processInfoCreator.Create(BENCH_ENGINE_COMMAND, binaryPath); 
        var (output, _) = _runner.RunProcess(info);
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

    public Process? RunFastchess(string arguments)
    {
        var processStartInfo = _processInfoCreator.Create(arguments, FastchessDependency.FASTCHESS_BINARY_FILE_PATH);
        var process = Process.Start(processStartInfo);
        return process;
    }
}