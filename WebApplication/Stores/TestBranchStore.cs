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

    /// <summary>
    /// Sets a bench for a testbranch -- used for autobenched resolved tests. 
    /// </summary>
    public async Task SetTestBranchBench(int testId, int autobenchStateBench)
    {
        await Context.TestBranches
            .Include(t => t.TestBranchOf)
            .Where(tb => tb.TestBranchOf!.Id == testId)
            .ExecuteUpdateAsync(props => props.SetProperty(tb => tb.Bench, autobenchStateBench));
    }
}