using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class TestBranchStore : Store<TestBranch>
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public TestBranchStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory) {}

    /// <inheritdoc /> 
    protected override DbSet<TestBranch> GetDbSet() => Context.TestBranches;
    
}