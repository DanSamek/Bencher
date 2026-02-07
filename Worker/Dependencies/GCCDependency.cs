using System.Text.RegularExpressions;
using Worker.ProcessOperations;

namespace Worker.Dependencies;

public partial class GCCDependency : ICompilerDependency
{
    private readonly IProcessRunner _runner;
    private readonly ProcessStartInfoCreator _processInfoCreator;
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public GCCDependency(IProcessRunner runner, ProcessStartInfoCreator processInfoCreator)
    {
        _runner = runner;
        _processInfoCreator = processInfoCreator;
    }
    
    public bool Validate()
    {
        var command = "gcc -v";
        var process = _processInfoCreator.Create(command);
        var (_, error) = _runner.RunProcess(process); 
        return _regex.IsMatch(error ?? string.Empty);
    }

    public string ErrorMessage() => "Unable to resolve GCC dependency";
    public Compilers Compiler =>  Compilers.Gcc;
    
    [GeneratedRegex("gcc version")]
    private static partial Regex GccVersionRegex();
    
    private static readonly Regex _regex = GccVersionRegex();
}