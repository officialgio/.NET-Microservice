using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Play.Common.Settings;

namespace Play.Common.MassTransit;

/// <summary>
/// This class maintains concrete functionality for the IserviceCollection initialization to keep
/// everything abstracted and clean within the Program.cs file and initialize any general MassTransmit instances
/// of services.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// This will configure and register MassTransmit services.
    /// </summary>
    public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services, Action<IRetryConfigurator> configureRetries = null)
    {
        services.AddMassTransit(configure =>
        {
            // Any consumer classes that are in the assembly will be the a consumer
            configure.AddConsumers(Assembly.GetEntryAssembly());
            configure.UsingPlayEconomoyRabbitMq(configureRetries);
        });

        return services;
    }

    /// <summary>
    /// This is used to register the RabbitMq configurations such as apply a Host and Endpoints, along with an optional retry handler.
    /// </summary>
    public static void UsingPlayEconomoyRabbitMq(this IBusRegistrationConfigurator configure, Action<IRetryConfigurator> configureRetries = null)
    {
        configure.UsingRabbitMq((context, configurator) =>
        {
            // Get an instance of configuration using context
            var configuration = context.GetService<IConfiguration>();
            var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

            // Register an instance of the RabbitMQ Settings and apply necessary configurations
            var rabbnitMQSettings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
            configurator.Host(rabbnitMQSettings?.Host);
            configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings?.ServiceName, false));

            // Retry 3 times and wait 5 seconds within those retries.
            if (configureRetries is null)
            {
                configureRetries = (retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
            }

            configurator.UseMessageRetry(configureRetries);
        });
    }
}
