using log4net;
using Microsoft.AspNetCore.Identity;
using WebApplication.Data.Models;

namespace WebApplication.Services;

/// <summary>
/// Helper class for creating admin user from the environment variables.
/// </summary>
public class EnvAdminCreator
{
    private static readonly ILog _logger =  LogManager.GetLogger(typeof(EnvAdminCreator));
    private const string ENV_ADMIN_USERNAME = "ADMIN_USERNAME";
    private const string ENV_ADMIN_PASSWORD = "ADMIN_PASSWORD";
    private const string ENV_ADMIN_EMAIL = "ADMIN_EMAIL";
    
    /// <summary>
    /// Creates admin user from the environment variables.
    /// </summary>
    /// <exception cref="NullReferenceException">Occurs, when environment variables are not set.</exception>
    public async Task Create(IUserStore<ApplicationUser> store, UserManager<ApplicationUser> userManager)
    {
        var adminUsername = Environment.GetEnvironmentVariable(ENV_ADMIN_USERNAME);
        var adminPassword = Environment.GetEnvironmentVariable(ENV_ADMIN_PASSWORD);
        var adminEmail = Environment.GetEnvironmentVariable(ENV_ADMIN_EMAIL);
        if (adminUsername == null || adminPassword == null ||  adminEmail == null)
        {
            throw new NullReferenceException($"{ENV_ADMIN_USERNAME} or {ENV_ADMIN_PASSWORD} or {ENV_ADMIN_EMAIL} is not set in the environment variables.");
        }
        var user = new ApplicationUser
        {
            Tests = [],
            OpeningBooks = [],
            Role = UserRole.Admin
        };
        
        await ((IUserEmailStore<ApplicationUser>)store).SetEmailAsync(user, adminEmail, CancellationToken.None);
        await store.SetUserNameAsync(user, adminUsername, CancellationToken.None);
        var result = await userManager.CreateAsync(user, adminPassword);
        
        if (!result.Succeeded)
        {
            var reasons = result.Errors.Aggregate(string.Empty, (current, error) => current + " " + error.Description);
            _logger.Warn($"Unable to create admin user, {reasons}");
        }
    }
}