using System.Diagnostics;
using Newtonsoft.Json;

namespace Worker;

public static class Helper
{
    public static string ERROR_PREFIX = "[ERROR]";
    public static string INFO_PREFIX = "[INFO]";
    
    public static async Task<T?> Deserialize<T>(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var streamReader = new StreamReader(stream);
        await using var jsonTextReader = new JsonTextReader(streamReader);
        var serializer = new JsonSerializer();
        var result = serializer.Deserialize<T>(jsonTextReader);
        return result;
    }
    
    public static ProcessStartInfo CreateProcessStartInfo(string arguments, string fileName = "/bin/bash")
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

    public record RunResult(string? StandardOutput, string? StandardError);
    public static RunResult RunProcess(ProcessStartInfo processInfo)
    {
        using var process = Process.Start(processInfo);
        var output = process?.StandardOutput.ReadToEnd();
        var error = process?.StandardError.ReadToEnd();
        process?.WaitForExit();
        
        var result = new RunResult(output, error);
        return result;
    }
    
    public static string WithPrefix(this string message, string prefix)
        => $"{prefix}: {message}";
}