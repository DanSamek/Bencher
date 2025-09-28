using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class AutobenchStateStore : StoreBase
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public AutobenchStateStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory){}
    
    /// <summary>
    /// Loads an autobench state by a test id. 
    /// </summary>
    public AutobenchState? GetAutobenchStateByTestId(int testId)
        => Context.AutobenchStates.FirstOrDefault(x => x.TestId == testId);
    
    
}