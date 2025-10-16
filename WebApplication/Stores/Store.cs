using Microsoft.EntityFrameworkCore;
using WebApplication.Data;

namespace WebApplication.Stores;

/// <summary>
/// Base class for all stores.
/// Stores are used for entity updates.
/// </summary>
public abstract class Store<T> : IDisposable, IStore<T>
where T : class
{
    /// <summary>
    /// Db context 
    /// </summary>
    protected ApplicationDbContext Context { get; }
    
    /// <summary>
    /// .Ctor
    /// </summary>
    /// <param name="factory">Db context factory.</param>
    protected Store(IDbContextFactory<ApplicationDbContext> factory) => Context = factory.CreateDbContext();
    
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

    
    /// <inheritdoc />
    public T? GetById(int id)
    {
        var dbSet = GetDbSet();
        var result = dbSet.Find(id);
        return result;
    }

    /// <inheritdoc />
    public void Update(T entity)
    {
        var dbSet = GetDbSet();
        dbSet.Update(entity);
        SaveChanges();
    }
    
    /// <inheritdoc />
    public void Delete(T entity)
    {
        var dbSet = GetDbSet();
        dbSet.Remove(entity);
        SaveChanges();
    }

    protected abstract DbSet<T> GetDbSet();
}