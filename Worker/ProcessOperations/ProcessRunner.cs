using System.Diagnostics;

namespace Worker.ProcessOperations;

/// <summary>
/// Injectable class for process running.
/// </summary>
public class ProcessRunner : IProcessRunner
{ 
    public RunResult RunProcess(ProcessStartInfo processInfo)
    {
        using var process = Process.Start(processInfo);
        var output = process?.StandardOutput.ReadToEnd();
        var error = process?.StandardError.ReadToEnd();
        process?.WaitForExit();
        process?.Close();
        
        var result = new RunResult(output, error);
        return result;
    }

}