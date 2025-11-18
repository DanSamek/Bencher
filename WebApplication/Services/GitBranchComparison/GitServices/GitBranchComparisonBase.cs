using WebApplication.Data.Models;

namespace WebApplication.Services.GitBranchComparison;

/// <summary>
/// Base class for all git services.
/// </summary>
public abstract class GitBranchComparisonBase : IGitBranchComparison
{
    /// <summary>
    /// .Ctor
    /// </summary>
    protected GitBranchComparisonBase(string gitServiceKeyword)  => GitServiceKeyword = gitServiceKeyword;
    
    /// <summary>
    /// Keyword of the git service.
    /// </summary>
    public virtual string GitServiceKeyword { get; }
    
    /// <inheritdoc /> 
    public abstract string GetDiffUrl(Test test);
}