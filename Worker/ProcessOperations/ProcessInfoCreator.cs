using System.Diagnostics;

namespace Worker.ProcessOperations;

/// <summary>
/// Injectable class for creating of the process.
/// </summary>
public class ProcessStartInfoCreator
{
    public ProcessStartInfo Create(string arguments, string fileName = "/bin/bash")
    {
        var processInfo = new ProcessStartInfo();
        processInfo.FileName = fileName;
        processInfo.UseShellExecute = false;
        processInfo.RedirectStandardOutput = true;
        processInfo.RedirectStandardError = true;
        processInfo.CreateNoWindow = true;
        processInfo.Arguments = fileName == "/bin/bash" ? $"-c \"{arguments}\"" :  arguments;
        return processInfo;
    }
}