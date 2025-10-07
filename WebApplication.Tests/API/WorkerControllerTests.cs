using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using WebApplication.API;
using WebApplication.Data.Models;
using WebApplication.Stores;

namespace WebApplication.Tests.API;

public class WorkerControllerTests
{
    private WorkerController _controller;
    
    [SetUp]
    public void Setup()
    {
        var factory = new TestContextFactory();
        // Prepare controller.
        _controller = new WorkerController(new UserStore(factory), new WorkerLogStore(factory), new PentaStore(factory), 
                                           new TestStore(factory), new TestBranchStore(factory), new AutobenchStateStore(factory));
        
        // Insert user with an access token.
        using var context = factory.CreateDbContext();
        var accessToken = "123456789";
        context.Users.Add(new ApplicationUser
        {
            Tests = [],
            OpeningBooks = [],
            PasswordHash = "123456",
            UserName = "test",
            AccessToken = accessToken
        });
        context.SaveChanges();
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();
        _controller.HttpContext.Request.Headers.Add(new (Shared.WORKER_REQUEST_HEADER, accessToken));
        
        // Create 

    }

    [Test]
    public void Test()
    {
    }
    
}