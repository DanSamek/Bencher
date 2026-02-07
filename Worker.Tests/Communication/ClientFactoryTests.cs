using Worker.UI;

namespace Worker.Tests.Communication;

[TestFixture]
public class ClientFactoryTests
{
    /// <summary>
    /// Tests <see cref="ClientFactory.Get"/>.
    /// </summary>
    [Test]
    public void Get()
    {
        var factory = new ClientFactory(new RunnerOptions
        {
            WebApplicationUrl = "http://test.xyz",
            UserToken = "A2AS1S5S8"
        });

        using var client = factory.Get();
        Assert.That(client, Is.Not.Null);
        Assert.That(client.BaseAddress, Is.Not.Null);
        Assert.That(client.BaseAddress.ToString(),Is.EqualTo("http://test.xyz/"));
        Assert.That(client.DefaultRequestHeaders.Contains(Shared.Constants.WORKER_REQUEST_HEADER), Is.True);
        Assert.That(client.DefaultRequestHeaders.GetValues(Shared.Constants.WORKER_REQUEST_HEADER).First(), Is.EqualTo("A2AS1S5S8"));
    }
}