using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;


namespace WebApplication.Stores;

public class UserStore : StoreBase
{
    public UserStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory) { }

    // Testing the idea of user stores.
    public List<ApplicationUser> GetAllUsers()
    {
        var result = Context.Users.ToList();
        return result;
    }
    
    public void DeleteUser(ApplicationUser user)
    {
        Context.Users.Remove(user);
        SaveChanges();
    }
}