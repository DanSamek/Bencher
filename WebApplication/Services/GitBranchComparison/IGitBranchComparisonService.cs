using WebApplication.Data.Models;

namespace WebApplication.Services.GitBranchComparison;

/// <summary>
/// Interface for the service to select correct git branch comparison implementation.
/// </summary>
public interface IGitBranchComparisonService
{
    /// <summary>
    /// Returns url for the comparison.
    /// </summary>
    DiffUrlResult GetDiffUrl(Test test);
}