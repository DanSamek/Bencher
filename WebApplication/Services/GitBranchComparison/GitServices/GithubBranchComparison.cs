using WebApplication.Data.Models;

namespace WebApplication.Services.GitBranchComparison;

/// <summary>
/// Github implementation for the diff.
/// </summary>
public class GithubBranchComparison : GitBranchComparisonBase
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public GithubBranchComparison() : base(GitServicesBranchComparisonKeywords.GITHUB_KEYWORD) { }
    
    /// <inheritdoc /> 
    public override string GetDiffUrl(Test test)
    {
        var url= $"{test.Engine.GitUrl}/compare/{test.BaseBranch.Name}..{test.TestBranch.Name}";
        return url;
    }
    
}