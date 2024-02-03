
using System;
using MongoDB.Driver;
using Play.Catalog.Service.Entities;

namespace Play.Catalog.Service.Repositories;

/// <summary>
/// This class provides a generic Singleton instance that can be used by other Microservices.
/// Ensure that the type who will use this implements IEntity <see cref="IEntity"/>
/// </summary>
public class MongoRepository<T> : IRepository<T> where T : IEntity
{
    /// <summary>
    /// A read-only collection that keeps database instance collection.
    /// </summary>
    private readonly IMongoCollection<T> dbCollection;

    /// <summary>
    /// A builder that will helper filter and query within the dbCollection.
    /// </summary>
    private readonly FilterDefinitionBuilder<T> filterBuilder = Builders<T>.Filter;

    public MongoRepository(IMongoDatabase database, string collectionName)
    {
        dbCollection = database.GetCollection<T>(collectionName);
    }

    public async Task<IReadOnlyCollection<T>> GetAllAsync()
    {
        return await dbCollection.Find(filterBuilder.Empty).ToListAsync();
    }

    public async Task<T> GetAsync(Guid id)
    {
        FilterDefinition<T> filter = filterBuilder.Eq(entity => entity.id, id);
        return await dbCollection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(T entity)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(entity));
        await dbCollection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(T entity)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(entity));
        FilterDefinition<T> filter = filterBuilder.Eq(existingEntity => existingEntity.id, entity.id);
        await dbCollection.ReplaceOneAsync(filter, entity);
    }

    public async Task RemoveAsync(Guid id)
    {
        FilterDefinition<T> filter = filterBuilder.Eq(entity => entity.id, id);
        await dbCollection.DeleteOneAsync(filter);
    }
}
 