using System.Text.RegularExpressions;

namespace Worker.Dependencies;

public partial class ClangDependency : ICompilerDependency
{
    public bool Validate()
    {
        var command = "clang -v";
        var processInfo = Helper.CreateProcessStartInfo(command);
        var (_, error) = Helper.RunProcess(processInfo); 
        return _regex.IsMatch(error ?? string.Empty);
    }

    public string ErrorMessage() => "Unable to resolve Clang dependency";

    public Compilers Compiler => Compilers.Clang;

    [GeneratedRegex("clang version")]
    private static partial Regex ClangVersionRegex();
    private static readonly Regex _regex = ClangVersionRegex();
}