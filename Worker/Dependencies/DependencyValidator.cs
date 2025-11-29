namespace Worker.Dependencies;

/// <summary>
/// Validates if dependencies are installed at the worker.
/// </summary>
public static class DependencyValidator
{
    private static readonly IReadOnlyList<IValidatableDependency> _validatableDependencies = new List<IValidatableDependency> 
    {
        new GitDependency()
    };

    private static readonly IReadOnlyList<ICompilerDependency> _compilerDependencies =
        new List<ICompilerDependency>
        {
            new ClangDependency(),
            new GCCDependency(),
        };

 
    public record ValidationResult(Compilers Compilers, ErrorTrace Trace);
    public static ValidationResult Validate()
    {
        var trace = new ErrorTrace();
        
        foreach (var dependency in _validatableDependencies)
        {
            var validationResult = dependency.Validate();
            if (!validationResult)
            {
                trace.AddError(dependency.ErrorMessage());
            }
        }

        var compilers = Compilers.None;
        foreach (var dependency in _compilerDependencies)
        {
            var validationResult = dependency.Validate();
            if (!validationResult) continue;
            
            compilers |= dependency.Compiler;
        }
        
        if (compilers == Compilers.None)
        {
            trace.AddError($"Missing compilers: {Compilers.Gcc} or {Compilers.Clang}");
        }
        
        var result = new ValidationResult(compilers, trace);
        return result;
    }
}