namespace Worker.Dependencies;

public interface ICompilerDependency : IValidatableDependency
{
    public Compilers Compiler { get; }
}