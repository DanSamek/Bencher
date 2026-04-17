using Newtonsoft.Json;
using Shared;

namespace Worker;

public static class Helper
{
    public const bool DEVELOPMENT = false;
    public const string ERROR_PREFIX = "[ERROR]";
    public const string INFO_PREFIX = "[INFO]";
    
    public static T? Deserialize<T>(HttpResponseMessage response)
    {
        using var stream = response.Content.ReadAsStream();
        using var streamReader = new StreamReader(stream);
        using var jsonTextReader = new JsonTextReader(streamReader);
        var serializer = new JsonSerializer();
        var result = serializer.Deserialize<T>(jsonTextReader);
        return result;
    }
    
   
    public static string WithPrefix(this string message, string prefix)
        => $"{prefix}: {message}";
    
    public static string EngineBinary(DirectoryInfo directoryInfo)
        => $"{directoryInfo.FullName}/{Constants.BENCHER_BINARY_FOLDER}/{Constants.BENCHER_BINARY_NAME}";
    
}
