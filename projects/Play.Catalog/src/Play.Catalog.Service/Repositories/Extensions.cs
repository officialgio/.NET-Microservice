using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Settings;

namespace Play.Catalog.Service.Repositories;

/// <summary>
/// This class maintains concrete functionality for the IserviceCollection initialization to keep
/// everything abstracted and clean within the Program.cs file and intialize any general instances
/// of services.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// This will create a IMongoDatabase that will be used to initially a generic
    /// MongoRepository for any Entity.
    /// </summary>
	public static IServiceCollection AddMongo(this IServiceCollection services)
	{
        // Keep original types when inserting MongoDB Docouments
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));    

        // Construct the MongoDB Client
        services.AddSingleton(serviceProvider => {
            // Get configuration service from insfrastructure
            var configuration = serviceProvider.GetService<IConfiguration>();

            // Bindings
            var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            var mongoDbSettings = configuration.GetSection(nameof(MongoDBSettings)).Get<MongoDBSettings>();

            // Init MongoClient
            var mongoClient = new MongoClient(mongoDbSettings?.ConnectionString);
            return mongoClient.GetDatabase(serviceSettings?.ServiceName);
        });

        return services;
    }

    /// <summary>
    /// Generic MongoRepository for Entities who implement IEntity 
    /// </summary>
    public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services, string collectionName) where T : IEntity
    {
        services.AddSingleton<IRepository<T>>(serviceProvider =>
        {
            // This call will work because we've registered the IMongoDatabase beforehand
            var database = serviceProvider.GetService<IMongoDatabase>();
            return new MongoRepository<T>(database, collectionName);
        });

        return services;
    }
}
