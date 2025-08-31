using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApplication.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Engine> Engines { get; set; }
    public DbSet<TestBranch> TestBranches { get; set; }
    public DbSet<Test> Tests { get; set; }
    public DbSet<OpeningBook> OpeningBooks { get; set; }
    public DbSet<SprtSettings> SprtSettings { get; set; }
    public DbSet<Penta> Pentas { get; set; }
    public DbSet<Error> Errors { get; set; }
    public DbSet<WorkerLog> WorkerLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // TODO.
        base.OnModelCreating(builder);
    }
}
