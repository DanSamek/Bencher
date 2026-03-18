using System.Text.RegularExpressions;
using Worker.ProcessOperations;

namespace Worker.Dependencies;

public partial class ClangDependency : ICompilerDependency
{
    private readonly IProcessRunner _runner;
    private readonly ProcessStartInfoCreator _processInfoCreator;
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public ClangDependency(IProcessRunner runner, ProcessStartInfoCreator processInfoCreator)
    {
        _runner = runner;
        _processInfoCreator = processInfoCreator;
    }
    
    public bool Validate()
    {
        var command = "clang -v";
        var processInfo = _processInfoCreator.Create(command);
        var (_, error) = _runner.RunProcess(processInfo); 
        return _regex.IsMatch(error ?? string.Empty);
    }

    public string ErrorMessage() => "Clang is not installed";

    public Compilers Compiler => Compilers.Clang;

    [GeneratedRegex("clang version")]
    private static partial Regex ClangVersionRegex();
    private static readonly Regex _regex = ClangVersionRegex();
}