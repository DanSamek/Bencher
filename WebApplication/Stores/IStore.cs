namespace WebApplication.Stores;

/// <summary>
/// Base interface for stores.
/// </summary>
/// <typeparam name="T">Entity</typeparam>
public interface IStore<T>
where T : class
{
    /// <summary>
    /// Gets entity by an id - tracked by EF.
    /// </summary>
    T? GetById(int id);

    /// <summary>
    /// Updates the entity.
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Deletes an entity. 
    /// </summary>
    void Delete(T entity);
    
    /// <summary>
    /// Entity, that will be attached in the store. 
    /// </summary>
    void Attach(object entity);
}