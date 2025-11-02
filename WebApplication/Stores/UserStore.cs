using System.Text;
using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;


namespace WebApplication.Stores;

public class UserStore : Store<ApplicationUser>
{
    public UserStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory) { }
    
    /// <inheritdoc /> 
    protected override DbSet<ApplicationUser> GetDbSet() =>  Context.Users;
    
    /// <summary>
    /// Verifies that users access token exists. 
    /// </summary>
    public bool DoesUserTokenExists(string accessToken) 
        => Context.Users.Any(u => u.AccessToken == accessToken);
    
    /// <summary>
    /// Gets user by accessToken.
    /// </summary>
    public ApplicationUser? GetUserByAccessToken(string accessToken)
        => Context.Users.FirstOrDefault(u => u.AccessToken == accessToken);
    
    /// <summary>
    /// Creates an access token for a user.
    /// </summary>
    /// <param name="userId">Id of the user.</param>
    public void CreateAccessToken(string userId)
    {
        string accessToken;
        while (true)
        {
            accessToken = GenerateAccessToken();
            var anyUserWithAccessToken = GetDbSet().Any(u => u.AccessToken == accessToken);
            if (!anyUserWithAccessToken) break;
        }

        GetDbSet()
            .Where(u => u.Id == userId)
            .ExecuteUpdate(spc => spc.SetProperty(u => u.AccessToken, accessToken));
    }

    /// <summary>
    /// Removes an access token for a user.
    /// </summary>
    /// <param name="userId">Id of the user.</param>
    public void RemoveAccessToken(string userId)
    {
        GetDbSet()
            .Where(u => u.Id == userId)
            .ExecuteUpdate(spc => spc.SetProperty(u => u.AccessToken, (string?)null));
    }

    /// <summary>
    /// Gets user by id.
    /// </summary>
    public ApplicationUser? GetById(string id)
    {
        return GetDbSet().AsNoTracking().FirstOrDefault(u => u.Id == id);
    }

    private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private string GenerateAccessToken()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < 16; i++)
        {
            sb.Append(CHARS[Random.Shared.Next(0, CHARS.Length)]);
        }
        
        var accessToken = sb.ToString();
        return accessToken;
    }
}