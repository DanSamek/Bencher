using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication.Data.Models;

namespace WebApplication.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Engine> Engines { get; set; }
    public DbSet<TestBranch> TestBranches { get; set; }
    public DbSet<Test> Tests { get; set; }
    public DbSet<OpeningBook> OpeningBooks { get; set; }
    public DbSet<SprtSettings> SprtSettings { get; set; }
    public DbSet<Penta> Pentas { get; set; }    
    public DbSet<TestError> TestErrors { get; set; }
    public DbSet<WorkerLog> WorkerLogs { get; set; }
    public DbSet<AutobenchState> AutobenchStates { get; set; }
    
    public DbSet<Error> WorkerErrors { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        #region User
        builder.Entity<ApplicationUser>()
            .HasMany(x => x.Engines)
            .WithOne(x => x.User)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ApplicationUser>()
            .HasMany(x => x.Tests)
            .WithOne(x => x.User)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Entity<ApplicationUser>()
            .HasMany(x => x.WorkerLogs)
            .WithOne(x => x.User)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion

        #region Test

        builder.Entity<Test>()
            .HasMany(x => x.Errors)
            .WithOne(x => x.Test)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Test>()
            .HasMany(x => x.WorkerLogs)
            .WithOne(x => x.Test)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<Test>()
            .HasOne(x => x.BaseBranch)
            .WithOne(x => x.BaseBranchOf)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<Test>()
            .HasOne(x => x.TestBranch)
            .WithOne(x => x.TestBranchOf)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<Test>()
            .HasOne(x => x.Penta)
            .WithOne(x => x.Test)
            .HasForeignKey<Penta>(x => x.TestId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<Test>()
            .HasOne(t => t.AutobenchState)
            .WithOne(abs => abs.Test)
            .HasForeignKey<AutobenchState>(a => a.TestId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.Entity<Test>()
            .HasIndex(t => t.TestBranchId)
            .IsUnique(false);
        
        builder.Entity<Test>()
            .HasIndex(t => t.BaseBranchId)
            .IsUnique(false);
        #endregion

        #region Engine

        builder.Entity<Engine>()
            .HasMany(x => x.Branches)
            .WithOne(x => x.Engine)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<Engine>()
            .HasMany(x => x.Tests)
            .WithOne(x => x.Engine)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion

        #region WorkerLog

        builder.Entity<WorkerLog>()
            .HasMany(x => x.Errors)
            .WithOne(x => x.WorkerLog)
            .OnDelete(DeleteBehavior.Cascade);
        
        #endregion
        
        #region OpeningBook
        
        builder.Entity<OpeningBook>()
            .HasOne(ob => ob.Data)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
        
        #endregion

        #region Error
        
        builder.Entity<TestError>()
            .HasOne(e => e.Log)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
        
        #endregion
        
    }
}
