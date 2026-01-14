namespace Worker.Dependencies;

/// <summary>
/// Tries to resolve dependencies - download, build..
/// </summary>
public static class DependencyResolver
{
    private static readonly IReadOnlyList<IResolvableDependency> _resolvableDependencies =
        new List<IResolvableDependency>
        {
            new FastchessDependency()
        };
    
    public static ErrorTrace TryResolve(Compilers compilers)
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