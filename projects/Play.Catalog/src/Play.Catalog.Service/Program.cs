using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Play.Catalog.Service.Repositories;
using Play.Catalog.Service.Settings;

ServiceSettings serviceSettings;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

// Add services to the container.

// Keep original types when inserting MongoDB Docouments
BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

// bindings
serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

// Construct the MongoDB Client
builder.Services.AddSingleton(serviceProvider => {
    var mongoDbSettings = configuration.GetSection(nameof(MongoDBSettings)).Get<MongoDBSettings>();
    var mongoClient = new MongoClient(mongoDbSettings?.ConnectionString);
    return mongoClient.GetDatabase(serviceSettings?.ServiceName);
});

builder.Services.AddSingleton<IItemsRepository, ItemsRepository>();

builder.Services.AddControllers(options =>
{
    // Avoid ASP.NET Core Removing Async Suffix at Runtime
    options.SuppressAsyncSuffixInActionNames = false;    
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
