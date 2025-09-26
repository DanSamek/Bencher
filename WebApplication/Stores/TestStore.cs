using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Data.Models;

namespace WebApplication.Stores;

public class TestStore : StoreBase
{
    public TestStore(IDbContextFactory<ApplicationDbContext> factory) : base(factory) { }

    /// <summary>
    /// Gets test by an id.
    /// Note, includes <see cref="Penta" />
    /// </summary>
    public Test? GetById(int id) 
        => Context.Tests
            .Include(t => t.Penta)
            .FirstOrDefault(t=> t.Id == id);
}