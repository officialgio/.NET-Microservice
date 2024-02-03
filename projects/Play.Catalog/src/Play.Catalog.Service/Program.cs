using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Repositories;
using Play.Catalog.Service.Settings;

ServiceSettings serviceSettings;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

// Add services to the container.
serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

// Init Mongo Instance for Items
builder.Services
    .AddMongo()
    .AddMongoRepository<Item>("items");

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
