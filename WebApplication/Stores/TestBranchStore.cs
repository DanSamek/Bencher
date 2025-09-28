using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class TestBranchStore : StoreBase
{
    /// <summary>
    /// .Ctor
    /// </summary>
    public TestBranchStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory) {}
    
    /// <summary>
    /// Returns test branch by id.
    /// </summary>
    public TestBranch? GetById(int id) => Context.TestBranches.Find(id);
}