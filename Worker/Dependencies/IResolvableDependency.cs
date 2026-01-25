namespace Worker.Dependencies;

/// <summary>
/// Interface for the dependency, that can be automatically resolved.
/// </summary>
public interface IResolvableDependency : IValidatableDependency
{
    public bool TryResolve(Compilers compilers);
}