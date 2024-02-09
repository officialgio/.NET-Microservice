using MassTransit;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Settings;
using Play.Common.MongoDB;
using Play.Common.Settings;

ServiceSettings serviceSettings;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

// Add services to the container.
serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

// Init Mongo Instance for Items
builder.Services
    .AddMongo()
    .AddMongoRepository<Item>("items");

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, configurator) => 
    { 
        // Register an instance of the RabbitMQ Settings and apply necessary configurations
        var rabbnitMQSettings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
        configurator.Host(rabbnitMQSettings.Host);
        configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));
    });
});

// Start the RabbitMQ Service (deprecated - no need for this!)
// builder.Services.AddMassTransitHostedService();

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
