using System.Net;
using MassTransit;
using Play.Catalog.Service.Entities;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Common.Settings;

ServiceSettings serviceSettings;

var builder = WebApplication.CreateBuilder(args);

const string AllowedOriginSetting = "AllowedOrigin";

IConfiguration configuration = builder.Configuration;

// Add services to the container.
serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

// Init Mongo Instance for Items
// Start the RabbitMQ Service
builder.Services
    .AddMongo()
    .AddMongoRepository<Item>("items")
    .AddMassTransitWithRabbitMq();

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

    // Cors Middleware
    app.UseCors(builder =>
    {
        // use configuration object b/c the object contains all the data from the appsettings automatically
        // by the ASP.NET Core runtime.
        builder
        .WithOrigins(configuration[AllowedOriginSetting])
        .AllowAnyHeader()
        .AllowAnyMethod(); 
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
