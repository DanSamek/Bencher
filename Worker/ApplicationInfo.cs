using System.Text;

namespace Worker;

/// <summary>
/// Simple application information's.
/// </summary>
public static class ApplicationInfo
{
    private static string VERSION_ID => "0.3";
    private static string AUTHORS => "Daniel Samek";
    private static ErrorTrace _errorTrace = new();

    public static void Display()
    {
        Console.WriteLine("Bencher - worker");
        Console.WriteLine($"Version: {VERSION_ID}");
        Console.WriteLine($"Authors: {AUTHORS}");
        Console.WriteLine("\n");
    }

    public static void ShowLoggedUser(string username)
    {
        var lineLength= username.Length + "Logged as ".Length;
        var sb = new StringBuilder();
        while (lineLength-- > 0) sb.Append('-');
        
        var line = sb.ToString();
        Console.WriteLine(line);
        Console.WriteLine($"Logged as {username}");
        Console.WriteLine(line);
    }

    public static void Waiting(int waitSeconds)
    {
        _errorTrace.AddInfo($"Waiting {waitSeconds} seconds before communication...");
    }

    public static void ShowUnableToLogin(string webApplicationUrl)
    {
        _errorTrace.AddError($"Unable to login to the {webApplicationUrl}");
    }

    public static void Display(string message)
    {
        Console.WriteLine(message);
    }

    public static void Stopping()
    {
         _errorTrace.AddInfo("Stopping application...");
    }
}
