using System.Text;
using Shared;
using Shared.Dtos.Responses;
using Worker.TestProcessors.GameTestProcessor;

namespace Worker.Tests.TestProcessors.GameTestProcessor;

[TestFixture]
public class ExtensionsTests
{
    /// <summary>
    /// Tests <see cref="StringExtensions.Tm"/> - success parsing.
    /// </summary>
    [Test]
    public void Tm_Success()
    {
        var timeManagement = "5+0.05";
        var (seconds, increment) = timeManagement.Tm();
        Assert.That(seconds, Is.EqualTo(5));
        Assert.That(increment, Is.EqualTo(0.05));
    }
    
    /// <summary>
    /// Tests <see cref="StringExtensions.Tm"/> - invalid string input.
    /// </summary>
    [Test]
    public void Tm_Failure()
    {
        var timeManagement = "abcd";
        Assert.Throws<Exception>(() => timeManagement.Tm());
    }

    /// <summary>
    /// Tests <see cref="StringBuilderExtensions.AddEngine"/>.
    ///     - we expect that engine param (-engine proto=uci $cmdName) is added to the StringBuilder.
    /// </summary>
    [Test]
    public void AddEngine()
    {
        var sb = new StringBuilder();
        sb.AddEngine("./stockfish");
        var result = sb.ToString();
        Assert.That(result, Is.EqualTo("-engine proto=uci ./stockfish "));
    }
    
    /// <summary>
    /// Tests <see cref="StringBuilderExtensions.AddArgument"/>.
    ///     - we expect that argument is added to the StringBuilder with additional " " in the end.
    /// </summary>
    [Test]
    public void AddArgument()
    {
        var sb = new StringBuilder();
        sb.AddArgument("argument");
        var result = sb.ToString();
        Assert.That(result, Is.EqualTo("argument "));
    }

    /// <summary>
    /// Tests <see cref="StringBuilderExtensions.AddOpeningBook"/>.
    ///     - we expect that opening book (-openings order=random file=path format=$format) is added to the StringBuilder.
    /// </summary>
    [TestCase( OpeningBookType.EPD, "epd")]
    [TestCase( OpeningBookType.PGN, "pgn")]
    public void AddOpeningBook(OpeningBookType type, string expectedFormat)
    {
        var sb = new StringBuilder();
        var filename = $"uho.{expectedFormat}";
        sb.AddOpeningBook($"/tmp/openingbook/{filename}", new OpeningBookDto(filename, [], type));
        var result = sb.ToString();
        Assert.That(result, Is.EqualTo($"-openings order=random file=/tmp/openingbook/{filename} format={expectedFormat} "));
    }
}