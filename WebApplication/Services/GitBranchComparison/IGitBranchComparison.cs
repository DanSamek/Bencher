using WebApplication.Data.Models;

namespace WebApplication.Services.GitBranchComparison;

/// <summary>
/// Interface for the git comparison of the branches [diff].
/// </summary>
public interface IGitBranchComparison
{
    /// <summary>
    /// Keyword of the git service.
    /// </summary>
    string GitServiceKeyword { get; }
    
    /// <summary>
    /// Returns url for the comparison.
    /// </summary>
    string GetDiffUrl(Test test);
}