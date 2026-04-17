using System.Text;

namespace Worker.UI;

/// <summary>
/// Simple application information's.
/// </summary>
public static class ApplicationInfo
{
    private static string VERSION_ID => "1.0";
    private static string AUTHORS => "Daniel Samek";
    private static readonly ErrorTrace _errorTrace = new();

    private const string NAME =
        "__________                     .__                            __      __             __                 \n" +
        "\\______   \\ ____   ____   ____ |  |__   ___________          /  \\    /  \\___________|  | __ ___________ \n" +
        " |    |  _// __ \\ /    \\_/ ___\\|  |  \\_/ __ \\_  __ \\  ______ \\   \\/\\/   /  _ \\_  __ \\  |/ // __ \\_  __ \\\n" +
        " |    |   \\  ___/|   |  \\  \\___|   Y  \\  ___/|  | \\/ /_____/  \\        (  <_> )  | \\/    <\\  ___/|  | \\/\n" +
        " |______  /\\___  >___|  /\\___  >___|  /\\___  >__|              \\__/\\  / \\____/|__|  |__|_ \\\\___  >__|   \n" +
        "        \\/     \\/     \\/     \\/     \\/     \\/                       \\/                   \\/    \\/       ";
    
    public static void Display()
    {
        Console.WriteLine(NAME);
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
