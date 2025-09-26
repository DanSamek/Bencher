using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class PentaStore : StoreBase
{
    /// <summary>
    /// .Ctor
    /// </summary>
    /// <param name="factory"></param>
    public PentaStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory) { }
    
    /// <summary>
    /// Thread-safe update of the pentanomial values.
    /// </summary>
    public async Task UpdatePenta(int testId, int ll, int ld, int dd, int wl, int wd, int ww)
    {
        await Context.Pentas
            .Include(t => t.Test)
            .Where(t => t.TestId == testId && t.Test.State == TestState.Running)
            .ExecuteUpdateAsync
            (
    spc => 
                 spc.SetProperty(p => p.Ll, p => p.Ll + ll)
                    .SetProperty(p => p.Ld, p => p.Ld + ld)
                    .SetProperty(p => p.Dd,p => p.Dd + dd)
                    .SetProperty(p => p.Wl, p => p.Wl + wl)
                    .SetProperty(p => p.Wd, p => p.Wd + wd)
                    .SetProperty(p => p.Ww, p => p.Ww + ww)
            );
    }
}