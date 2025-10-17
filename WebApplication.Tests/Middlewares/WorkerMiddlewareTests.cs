using Microsoft.AspNetCore.Http;
using Moq;
using WebApplication.API;
using WebApplication.Stores;
using WebApplication.Tests.Builders;

namespace WebApplication.Tests.Middlewares;

[TestFixture]
public class WorkerMiddlewareTests : TestBase
{
    /// <summary>
    /// Test on the request filter, for all "worker-api" paths [urls].
    /// </summary>
    [TestCase("/worker-api/some_endpoint", "1111", "1111", true)]
    [TestCase("/worker-api/some_endpoint", "1111", "2222", false)]
    [TestCase("/worker-api/some_endpoint_2", null, null, false)]
    [TestCase("/worker-api/some_endpoint_2", null, "1111", false)]
    [TestCase("/www/api", "1111", "2222", true)]
    [TestCase("/www/api", null, "2222", true)]
    [TestCase("/www/api", "1111", "1111", true)]
    public async Task PassTest(string requestUri, string? createdAccessToken, string? usedAccessToken, bool shouldPassMiddleware)
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateUser("test_user")
                .WithAccessToken(createdAccessToken)
                .Close()
            .CreateUser("test_user_2")
                .Close()
            .Close();
        
        var workerMiddleware = new WorkerMiddleware();
        var httpContext = new DefaultHttpContext();
        
        var userStore = new UserStore(Factory);
        
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(UserStore)))
            .Returns(userStore);
        httpContext.RequestServices = serviceProviderMock.Object; 
        httpContext.Request.Path = new PathString(requestUri);
        
        // Only if not null
        if (usedAccessToken is not null)
        {
            httpContext.Request.Headers[Shared.WORKER_REQUEST_HEADER] = usedAccessToken;
        }

        var wasInvokedNext = false;
        await workerMiddleware.InvokeAsync(httpContext, _ =>
        {
            wasInvokedNext = true;
            return Task.CompletedTask;
        });

        Assert.That(shouldPassMiddleware, Is.EqualTo(wasInvokedNext));
    }
}