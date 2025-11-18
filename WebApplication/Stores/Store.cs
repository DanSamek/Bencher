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
        Context.Dispose();
        GC.SuppressFinalize(this);
    }

    
    /// <inheritdoc />
    public virtual T? GetById(int id)
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
    public T AddRet(T entity)
    {
        var dbSet = GetDbSet();
        var result = dbSet.Add(entity);
        return result.Entity;
    }

    /// <inheritdoc />
    public void Delete(T entity)
    {
        var dbSet = GetDbSet();
        dbSet.Remove(entity);
        SaveChanges();
    }

    /// <inheritdoc /> 
    public void Add(T entity)
    {
        var dbSet = GetDbSet();
        dbSet.Add(entity);
        SaveChanges();
    }

    public void Attach(object entity) => Context.Attach(entity);

    protected abstract DbSet<T> GetDbSet();
}