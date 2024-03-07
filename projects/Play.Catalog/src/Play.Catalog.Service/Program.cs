using System.Net;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Play.Catalog.Service;
using Play.Catalog.Service.Entities;
using Play.Common.Identity;
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
    .AddMassTransitWithRabbitMq()
    .AddJwtBearerAuthentication();

// If you wan to read/write the Catalog Service
// you must be an admin role along with the respective claims.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.Read, policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireClaim("scope", "catalog.readaccess", "catalog.fullaccess");
    });

    options.AddPolicy(Policies.Write, policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireClaim("scope", "catalog.writeaccess", "catalog.fullaccess");
    });
});

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
        // use configuration[index<string>] object b/c the object c
        // ontains all the data from the appsettings automatically
        // by the ASP.NET Core runtime.
        builder
        .WithOrigins(configuration[AllowedOriginSetting])
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
