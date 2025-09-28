using Microsoft.EntityFrameworkCore;
using WebApplication.Data;

namespace WebApplication.Stores;

/// <summary>
/// Base class for all stores.
/// Stores are used for entity updates.
/// </summary>
public class StoreBase : IDisposable
{
    /// <summary>
    /// Db context 
    /// </summary>
    protected ApplicationDbContext Context { get; }
    
    /// <summary>
    /// .Ctor
    /// </summary>
    /// <param name="factory">Db context factory.</param>
    public StoreBase(IDbContextFactory<ApplicationDbContext> factory) => Context = factory.CreateDbContext();
    
    /// <summary>
    /// Saves all changes to a database.
    /// Note, when store is disposed, context is saved. 
    /// </summary>
    public void SaveChanges() => Context.SaveChanges(); 
    
    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        SaveChanges();
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
    
}