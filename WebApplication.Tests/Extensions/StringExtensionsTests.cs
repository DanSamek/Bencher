using WebApplication.Extensions;

namespace WebApplication.Tests.Extensions;

[TestFixture]
public class StringExtensionsTests
{
    /// <summary>
    /// Tests valid parsing of seconds from the string (xxx+yyy) where xxx are seconds.
    /// </summary>
    [TestCase("100+0.1", 100)]
    [TestCase("65000+0.1", 65000)]
    [TestCase("60+0.1", 60)]
    [TestCase("8+0.1", 8)]
    [TestCase("1+0.1", 1)]
    [TestCase("123456789+0.1", 123456789)]
    public void Seconds_Valid(string value, int expected)
    {
        var seconds = value.Seconds();
        Assert.That(seconds, Is.EqualTo(expected));
    }

    /// <summary>
    /// Tests invalid parsing of seconds from the string (xxx+yyy) where xxx are seconds.
    /// </summary>
    [TestCase("1234567890.1")]
    [TestCase("test")]
    public void Seconds_Invalid(string value)
    {
        Assert.Throws<Exception>(() => value.Seconds());
    }
}