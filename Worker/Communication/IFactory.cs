namespace Worker;

/// <summary>
/// Factory interface
/// </summary>
public interface IFactory<out T>
{
    /// <summary>
    /// Returns instance of the T.
    /// </summary>
    public T Get();
}