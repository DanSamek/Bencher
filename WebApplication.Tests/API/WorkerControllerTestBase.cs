using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using WebApplication.API;
using WebApplication.Stores;

namespace WebApplication.Tests.API;


/// <summary>
/// Base for all tests, that are using <see cref="WebApplication.API.WorkerController" />.
/// </summary>
public class WorkerControllerTestBase : TestBase
{
    protected WorkerController Controller { get; private set; }
    
    /// <summary>
    /// Creates a new instance of the controller.
    /// We can simulate each request individually.
    /// </summary>
    protected void RefreshController()
    {
        Controller = new WorkerController(new UserStore(Factory), new WorkerLogStore(Factory), new PentaStore(Factory), 
            CreateTestStore(), new TestBranchStore(Factory), new AutobenchStateStore(Factory), new OpeningBookStore(Factory));
    }
    
    protected void LoginAs(string username)
    {
        var user = Factory.CreateDbContext().Users.First(u => u.UserName == username);
        Controller.ControllerContext.HttpContext = new DefaultHttpContext();
        Controller.HttpContext.Request.Headers.Add(new KeyValuePair<string, StringValues>(Shared.WORKER_REQUEST_HEADER, user.AccessToken));
    }
    
    
    protected T? GetResponseValue<T, TRes>(IActionResult result)
        where TRes : ObjectResult
    {
        return (T?)((TRes)result).Value;
    }
}