using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class TestStore : StoreBase
{
    public TestStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory) { }
    
}