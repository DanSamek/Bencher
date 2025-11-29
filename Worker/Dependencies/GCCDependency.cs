using System.Text.RegularExpressions;

namespace Worker.Dependencies;

public partial class GCCDependency : ICompilerDependency
{
    public bool Validate()
    {
        var command = "gcc -v";
        var process = Helper.CreateProcessStartInfo(command);
        var (_, error) = Helper.RunProcess(process); 
        return _regex.IsMatch(error ?? string.Empty);
    }

    public string ErrorMessage() => "Unable to resolve GCC dependency";
    public Compilers Compiler =>  Compilers.Gcc;
    
    [GeneratedRegex("gcc version")]
    private static partial Regex GccVersionRegex();
    
    private static readonly Regex _regex = GccVersionRegex();
}