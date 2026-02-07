using Worker.ProcessOperations;
using Worker.UI;

namespace Worker.Dependencies;

/// <summary>
/// Validates if dependencies are installed at the worker.
/// </summary>
public class DependencyValidator
{
    private readonly IReadOnlyList<IValidatableDependency> _validatableDependencies;
    private readonly IReadOnlyList<ICompilerDependency> _compilerDependencies;
    
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public DependencyValidator(IProcessRunner runner, ProcessStartInfoCreator processInfoCreator)
    {
        _validatableDependencies = 
            new List<IValidatableDependency> 
            {
                new GitDependency(runner, processInfoCreator)
            };
        _compilerDependencies =
        new List<ICompilerDependency>
        {
            new ClangDependency(runner, processInfoCreator),
            new GCCDependency(runner,  processInfoCreator),
        };
    }

 
    public record ValidationResult(Compilers Compilers, ErrorTrace Trace);
    public ValidationResult Validate()
    {
        var trace = new ErrorTrace();
        trace.AddInfo("Validating installed dependencies");
        
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