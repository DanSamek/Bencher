using Worker.ProcessOperations;
using Worker.UI;

namespace Worker.Dependencies;

/// <summary>
/// Tries to resolve dependencies - download, build..
/// </summary>
public class DependencyResolver
{
    private readonly IReadOnlyList<IResolvableDependency> _resolvableDependencies;
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public DependencyResolver(IProcessRunner runner, ProcessStartInfoCreator processInfoCreator)
    {
        _resolvableDependencies  =
            new List<IResolvableDependency>
            {
                new FastchessDependency(runner, processInfoCreator)
            };
    }
    
    public ErrorTrace TryResolve(Compilers compilers)
    {
        var trace = new ErrorTrace();
        trace.AddInfo("Trying to resolve external dependencies");
        
        foreach (var dependency in _resolvableDependencies)
        {
            var validationResult = dependency.Validate();
            if (validationResult) continue;
            
            validationResult = dependency.TryResolve(compilers);
            if (validationResult) continue;
            
            trace.AddError(dependency.ErrorMessage());
        }
        return trace;
    }
}