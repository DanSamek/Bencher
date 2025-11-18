using WebApplication.Data.Models;

namespace WebApplication.Services.GitBranchComparison;

/// <summary>
/// Gitlab implementation for the diff.
/// </summary>
public class GitlabBranchComparison : GitBranchComparisonBase
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public GitlabBranchComparison() : base(GitServicesBranchComparisonKeywords.GITLAB_KEYWORD) { }

    /// <inheritdoc /> 
    public override string GetDiffUrl(Test test)
    {
        var url = $"{test.Engine.GitUrl}/compare?from={test.TestBranch.Name}&to={test.BaseBranch.Name}";
        return url;
    }
}