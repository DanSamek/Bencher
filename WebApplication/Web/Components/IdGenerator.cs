namespace WebApplication.Components.Components;

public static class IdGenerator
{
    private static uint _id = 0;
    
    /// <summary>
    /// Returns next id for components.
    /// </summary>
    public static uint GetId()
    {
        Interlocked.Increment(ref _id);
        return _id;
    }
}