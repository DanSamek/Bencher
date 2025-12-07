using WebApplication.Data.Models;
using WebApplication.Stores;
using WebApplication.Tests.Builders;

namespace WebApplication.Tests.Stores;

[TestFixture]
public class UserStoreTests : TestBase
{
    public override void Setup()
    {
        base.Setup();
        new DomainBuilder(Factory.CreateDbContext())
            .CreateUser("user-1")
                .WithAccessToken("abcdefg")
                .Close()
            .CreateUser("user-2")
                .Close()
            .CreateUser("user-3")
                .Close()
            .Close();
    }
    
    /// <summary>
    /// Tests <see cref="UserStore.DoesUserTokenExists"/>.
    /// </summary>
    [TestCase("abcdefg", true)]
    [TestCase("aaaaa", false)]
    public void DoesUserTokenExists(string accessToken, bool expected)
    {
        var store = new UserStore(Factory);
        var result= store.DoesUserTokenExists(accessToken);
        Assert.That(result, Is.EqualTo(expected));
    }

    /// <summary>
    /// Tests <see cref="UserStore.GetUserByAccessToken"/>.
    /// </summary>
    [TestCase("abcdefg", "user-1")]
    [TestCase("aaaaa", null)]
    public void GetUserByAccessToken(string accessToken, string? username)
    {
        var store = new UserStore(Factory);
        var user = store.GetUserByAccessToken(accessToken);
        Assert.That(user?.UserName, Is.EqualTo(username));
    }
    
    /// <summary>
    /// Tests <see cref="UserStore.CreateAccessToken"/>.
    /// </summary>
    [Test]
    public void CreateAccessToken()
    {
        var store = new UserStore(Factory);
        var user = GetByUsername("user-2", Factory);
        Assert.That(user.AccessToken, Is.EqualTo(null));
        store.CreateAccessToken(user.Id);
        
        user = GetByUsername("user-2", Factory);
        Assert.That(user.AccessToken, Is.Not.EqualTo(null));
    }
    
    /// <summary>
    /// Tests <see cref="UserStore.RemoveAccessToken"/>.
    /// </summary>
    [TestCase("user-1")]
    [TestCase("user-2")]
    public void RemoveAccessToken(string username)
    {
        var user = GetByUsername(username, Factory);
        var store = new UserStore(Factory);
        store.RemoveAccessToken(user.Id);
        user = GetByUsername(username, Factory);
        Assert.That(user.AccessToken, Is.EqualTo(null));
    }
    
    /// <summary>
    /// Tests <see cref="UserStore.GetById(string)"/>.
    /// </summary>
    [Test]
    public void GetById()
    {
        var user = GetByUsername("user-2", Factory);
        
        var store = new UserStore(Factory);
        var storeUser = store.GetById(user.Id)!;
        Assert.That(user.Id, Is.EqualTo(storeUser.Id));
        Assert.That(user.UserName, Is.EqualTo(storeUser.UserName));
    }

    /// <summary>
    /// Tests <see cref="UserStore.GetAllUsers"/>.
    /// </summary>
    [Test]
    public void GetAllUsers()
    {
        var store = new UserStore(Factory);
        var users = store
            .GetAllUsers()
            .OrderBy(u => u.UserName)
            .ToArray();
        
        Assert.That(users, Has.Length.EqualTo(3));
        Assert.That(users[0].UserName, Is.EqualTo("user-1"));
        Assert.That(users[1].UserName, Is.EqualTo("user-2"));
        Assert.That(users[2].UserName, Is.EqualTo("user-3"));
    }

    /// <summary>
    /// Tests <see cref="UserStore.SetRole"/>. 
    /// </summary>
    [TestCase(UserRole.Admin)]
    [TestCase(UserRole.Verified)]
    [TestCase(UserRole.NotVerified)]
    public void SetRole(UserRole role)
    {
        var store = new UserStore(Factory);
        var user = GetByUsername("user-2", Factory);
        store.SetRole(user.Id, role);

        user = GetByUsername("user-2", Factory);
        Assert.That(user.Role, Is.EqualTo(role));
    }

    /// <summary>
    /// Tests <see cref="UserStore.ConfirmUserEmail"/>.
    /// </summary>
    [Test]
    public void ConfirmUserEmail()
    {
        var store = new UserStore(Factory);
        var user = GetByUsername("user-1", Factory);
        Assert.That(user.EmailConfirmed, Is.False);
        store.ConfirmUserEmail(user.Id);
        
        user = GetByUsername("user-1", Factory);
        Assert.That(user.EmailConfirmed, Is.True);
    }

    /// <summary>
    /// Tests <see cref="UserStore.DeleteById"/>.
    /// </summary>
    [Test]
    public void DeleteById()
    {
        var store = new UserStore(Factory);
        var user = GetByUsername("user-1", Factory);
        store.DeleteById(user.Id);

        var exists = Factory
            .CreateDbContext()
            .Users
            .Any(u => u.Id == user.Id);
        Assert.That(exists, Is.False);
    }
    
    
    private ApplicationUser GetByUsername(string username, TestContextFactory factory) 
        => factory.CreateDbContext().Users.First(u => u.UserName == username); 
}