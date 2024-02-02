
using System;
using MongoDB.Driver;
using Play.Catalog.Service.Entities;

namespace Play.Catalog.Service.Repositories;

public class ItemsRepository
{
    private const string collectionName = "items";

    /// <summary>
    /// A read-only collection that keeps database instance collection.
    /// </summary>
    private readonly IMongoCollection<Item> dbCollection;

    /// <summary>
    /// A builder that will helper filter and query within the dbCollection.
    /// </summary>
    private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter;

    public ItemsRepository()
    {
        // Subject to change
        var mongoClient = new MongoClient("mongodb://localhost:27017");
        var database = mongoClient.GetDatabase("Catalog");
        dbCollection = database.GetCollection<Item>(collectionName);
    }

    public async Task<IReadOnlyCollection<Item>> GetAllAsync()
    {
        return await dbCollection.Find(filterBuilder.Empty).ToListAsync();
    }

    public async Task<Item> GetAsync(Guid id)
    {
        FilterDefinition<Item> filter = filterBuilder.Eq(entity => entity.id, id);
        return await dbCollection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Item entity)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(entity));
        await dbCollection.InsertOneAsync(entity);
    }

    public async Task updateAsync(Item entity)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(entity));
        FilterDefinition<Item> filter = filterBuilder.Eq(existingEntity => existingEntity.id, entity.id);
        await dbCollection.ReplaceOneAsync(filter, entity);
    }

    public async Task RemoveAsync(Guid id)
    {
        FilterDefinition<Item> filter = filterBuilder.Eq(entity => entity.id, id);
        await dbCollection.DeleteOneAsync(filter);
    }
}

