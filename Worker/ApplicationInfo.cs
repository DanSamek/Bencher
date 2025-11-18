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
}