namespace Worker.Dependencies;

public interface IResolvableDependency : IValidatableDependency
{
    public bool TryResolve(Compilers compilers);
}