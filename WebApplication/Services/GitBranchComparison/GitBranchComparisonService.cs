using WebApplication.Data.Models;

namespace WebApplication.Services.GitBranchComparison;

/// <summary>
/// Implementation of the IGitBranchComparisonService.
/// </summary>
public class GitBranchComparisonService : IGitBranchComparisonService
{
    private readonly List<IGitBranchComparison> _gitBranchComparisons = [];
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public GitBranchComparisonService([FromKeyedServices(nameof(GithubBranchComparison))] IGitBranchComparison githubBranchComparison,
                                      [FromKeyedServices(nameof(GitlabBranchComparison))] IGitBranchComparison gitlabBranchComparison)
    {
        _gitBranchComparisons.Add(githubBranchComparison);
        _gitBranchComparisons.Add(gitlabBranchComparison);
    }
    
    /// <inheritdoc /> 
    public DiffUrlResult GetDiffUrl(Test test)
    {
        foreach (var comparison in _gitBranchComparisons)
        {
            if (!test.Engine.GitUrl.Contains(comparison.GitServiceKeyword)) continue;
            
            var url = comparison.GetDiffUrl(test);
            var result = new DiffUrlResult(true, url);
            return result;
        }
        
        return new DiffUrlResult(false, string.Empty);
    }
}