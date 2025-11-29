using System.Text.RegularExpressions;

namespace Worker;

/// <summary>
/// Helper class for regexes, that are used.
/// </summary>
public static partial class Regexes
{
    // bench: 6479310, nps: 3896157 // TODO add that to the docs.
    [GeneratedRegex(@"bench:[^0-9]*(\d+).*nps:[^0-9]*(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex BenchNpsRegex();
    public static readonly Regex BenchRegex = BenchNpsRegex(); 
    
    [GeneratedRegex("error:.*", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GitErrorRefex();
    public static readonly Regex GitErrorRegex = GitErrorRefex();
}