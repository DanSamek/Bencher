using System.Diagnostics;

namespace Worker.ProcessOperations;

public record RunResult(string? StandardOutput, string? StandardError);
public interface IProcessRunner
{
    public RunResult RunProcess(ProcessStartInfo processInfo);
}