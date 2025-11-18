namespace WebApplication.Services.GitBranchComparison;

/// <summary>
/// Result of the <see cref="IGitBranchComparisonService.GetDiffUrl"/>
/// </summary>
/// <param name="Matched"></param>
/// <param name="DiffUrl"></param>
public record DiffUrlResult(bool Matched, string DiffUrl);