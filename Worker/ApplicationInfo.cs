namespace Worker;

/// <summary>
/// Simple application information's.
/// </summary>
public static class ApplicationInfo
{
    private static string VERSION_ID => "0.1";
    private static string AUTHORS => "Daniel Samek";

    public static void Display()
    {
        Console.WriteLine("Bencher - worker");
        Console.WriteLine($"Version: {VERSION_ID}");
        Console.WriteLine($"Authors: {AUTHORS}");
        Console.WriteLine("\n");
    }

    public static void ShowLoggedUser(string username)
    {
        Console.WriteLine($"Logged as {username}");
    }

    public static void Waiting(int waitSeconds)
    {
        Console.WriteLine($"Waiting {waitSeconds} seconds before communication...");
    }

    public static void ShowUnableToLogin(string webApplicationUrl)
    {
        Console.WriteLine($"Unable to login to the {webApplicationUrl}");
    }

    public static void Display(string message)
    {
        Console.WriteLine(message);
    }

    public static void Stopping()
    {
        Console.WriteLine("Stopping application...");
    }

}