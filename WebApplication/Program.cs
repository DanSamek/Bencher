using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication;
using WebApplication.Components.Account;
using WebApplication.Data;
using WebApplication.Data.Models;
using WebApplication.API;
using WebApplication.Services;
using WebApplication.Services.GitBranchComparison;
using WebApplication.Services.GmailSender;
using WebApplication.Stores;
using WebApplication.Web;

// Log4net.
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = Envloader.GetConnectionString();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddDbContextFactory<ApplicationDbContext>(lifetime: ServiceLifetime.Scoped);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, GmailSender>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
});

#region Stores

builder.Services.AddScoped<AutobenchStateStore>();
builder.Services.AddScoped<EngineStore>();
builder.Services.AddScoped<PentaStore>();
builder.Services.AddScoped<TestBranchStore>();
builder.Services.AddScoped<TestStore>();
builder.Services.AddScoped<UserStore>();
builder.Services.AddScoped<WorkerLogStore>();
builder.Services.AddScoped<OpeningBookStore>();
builder.Services.AddScoped<SprtSettingsStore>();
builder.Services.AddScoped<WorkerErrorStore>();
builder.Services.AddScoped<TestErrorStore>();

#endregion

#region WorkerAPI

builder.Services.AddScoped<IWorkerControllerService, WorkerControllerService>();
builder.Services.AddSingleton<CustomExceptionHandler>();
builder.Services.AddSingleton<WorkerMiddleware>();
builder.Services.AddHostedService<WorkerLogWatcher>();
builder.Services.AddControllers();

#endregion

#region GitComparison

builder.Services.AddSingleton<IGitBranchComparisonService, GitBranchComparisonService>();
builder.Services.AddKeyedSingleton<IGitBranchComparison, GithubBranchComparison>(nameof(GithubBranchComparison));
builder.Services.AddKeyedSingleton<IGitBranchComparison, GitlabBranchComparison>(nameof(GitlabBranchComparison));

#endregion

var app = builder.Build();

#region AdminCreation

using var scope = app.Services.CreateScope();

var userStore = (IUserStore<ApplicationUser>)scope.ServiceProvider.GetService(typeof(IUserStore<ApplicationUser>))!;
var userManager = (UserManager<ApplicationUser>)scope.ServiceProvider.GetService(typeof(UserManager<ApplicationUser>))!;

var creator = new EnvAdminCreator();
await creator.Create(userStore, userManager);

#endregion

// Force initialization of the sender.
app.Services.GetService<IEmailSender<ApplicationUser>>();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

#region WorkerApi

app.UseMiddleware<CustomExceptionHandler>();
app.UseMiddleware<WorkerMiddleware>();
app.MapControllers();

#endregion

app.Run();
