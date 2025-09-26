using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;


namespace WebApplication.Stores;

public class UserStore : StoreBase
{
    public UserStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory) { }
    
    /// <summary>
    /// Verifies that users access token exists. 
    /// </summary>
    public bool DoesUserTokenExists(string accessToken) => Context.Users.Any(u => u.AccessToken == accessToken);
    
    /// <summary>
    /// Gets user by accessToken.
    /// </summary>
    public ApplicationUser? GetUserByAccessToken(string accessToken) => Context.Users.FirstOrDefault(u => u.AccessToken == accessToken);
    
    /// <summary>
    /// Gets all users. 
    /// </summary>
    public List<ApplicationUser> GetAllUsers()
    {
        var result = Context.Users.ToList();
        return result;
    }
    
    /// <summary>
    /// Deletes user [only for testing purposes - TODO DeleteUserById]
    /// </summary>
    public void DeleteUser(ApplicationUser user)
    {
        Context.Users.Remove(user);
        SaveChanges();
    }
}