using System.Text.RegularExpressions;
using Worker.ProcessOperations;

namespace Worker.Dependencies;

public partial class GitDependency : IValidatableDependency
{
    private readonly IProcessRunner _runner;
    private readonly ProcessStartInfoCreator _processInfoCreator;
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public GitDependency(IProcessRunner runner, ProcessStartInfoCreator processInfoCreator)
    {
        _runner = runner;
        _processInfoCreator = processInfoCreator;
    }

    public bool Validate()
    {
        var processInfo = _processInfoCreator.Create("git -v");
        var (output, error) = _runner.RunProcess(processInfo);
        return string.IsNullOrEmpty(error) && _regex.IsMatch(output ?? string.Empty);
    }
    
    string IValidatableDependency.ErrorMessage() => "Git is not installed";
    
    [GeneratedRegex("git version")]
    private static partial Regex GitVersionRegex();
    
    private static readonly Regex _regex = GitVersionRegex();
}