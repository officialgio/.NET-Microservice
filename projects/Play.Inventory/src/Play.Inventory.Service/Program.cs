using Play.Common.MongoDB;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register and build the custom Mongo Database
builder
    .Services.AddMongo()
    .AddMongoRepository<InventoryItem>("inventoryitems");

Random jitter = new Random();

// Register CatalogClient to talk to REST Endpoints
// maxium retries ios 5 attemps with an exponential random duration within the attempts
// and wait at most 1 ssecond before giving up on the request.
builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5218");
})
.AddTransientHttpErrorPolicy(_builder => _builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
    5,
    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitter.Next(0, 1000)),
    onRetry: (outcome, timespan, retryAttempt) =>
    {
        // Get Service instance (NOTE: This is done as is for now...)
        var serviceProvider = builder.Services.BuildServiceProvider();
        serviceProvider.GetService<ILogger<CatalogClient>>() ?
            .LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
    }
))
.AddTransientHttpErrorPolicy(_builder =>  _builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
    3, // allow 3 requests
    TimeSpan.FromSeconds(15), // wait 15secs (won't allow calls within this time frame)
    onBreak: (outcome, timespan) =>
    {
        // When circuit breaker opens
        var serviceProvider = builder.Services.BuildServiceProvider();
        serviceProvider.GetService<ILogger<CatalogClient>>() ?
            .LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds...");
    },
    onReset: () =>
    {
        // When circuit cloeses
        var serviceProvider = builder.Services.BuildServiceProvider();
        serviceProvider.GetService<ILogger<CatalogClient>>()?.LogWarning($"Closing the circuit...");
    }
    ))
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));

builder.Services.AddControllers();
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
