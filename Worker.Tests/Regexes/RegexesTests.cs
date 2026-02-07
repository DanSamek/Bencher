namespace Worker.Tests.Regexes;

[TestFixture]
public class RegexesTests
{
    /// <summary>
    /// Tests successful path of the <see cref="Regexes.BenchRegex"/>
    /// </summary>
    [TestCase("bench: 10000   nps: 500000", 10000, 500000)]
    [TestCase("bench:    10000     nps:       500000", 10000, 500000)]
    public void BenchRegex_Success(string line, int expectedBench, int expectedNps)
    {
        var match = Worker.Regexes.BenchRegex.Match(line);
        Assert.That(match.Success, Is.True);
        Assert.That(match.Groups.Count, Is.EqualTo(3));
        Assert.That(int.Parse(match.Groups[1].Value), Is.EqualTo(expectedBench));
        Assert.That(int.Parse(match.Groups[2].Value), Is.EqualTo(expectedNps));
    }
    
    /// <summary>
    /// Tests failure path of the <see cref="Regexes.BenchRegex"/>
    /// </summary>
    [TestCase("bench: xxxx   nps: 500000")]
    [TestCase("bench:    10000     nps:       xxxxx")]
    public void BenchRegex_Failure(string line)
    {
        var match = Worker.Regexes.BenchRegex.Match(line);
        Assert.That(match.Success, Is.False);;
    }

    /// <summary>
    /// Tests successful path of the <see cref="Regexes.GitErrorRegex"/>
    /// </summary>
    [TestCase("    error: git error")]
    [TestCase("error: git error")]
    public void GitErrorRegex_Success(string line)
    {
        var match = Worker.Regexes.GitErrorRegex.Match(line);
        Assert.That(match.Success, Is.True);
    }
    
    /// <summary>
    /// Tests failure path of the <see cref="Regexes.GitErrorRegex"/>
    /// </summary>
    [TestCase("    x: git error")]
    [TestCase("git error")]
    public void GitErrorRegex_Failure(string line)
    {
        var match = Worker.Regexes.GitErrorRegex.Match(line);
        Assert.That(match.Success, Is.False);
    }
    
}