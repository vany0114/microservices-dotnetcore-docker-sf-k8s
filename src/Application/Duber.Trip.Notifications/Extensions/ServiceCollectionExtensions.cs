using Duber.Infrastructure.EventBus.RabbitMQ.IoC;
using Duber.Infrastructure.EventBus.ServiceBus.IoC;
using Duber.Trip.Notifications.Application.IntegrationEvents.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Duber.Trip.Notifications.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceBroker(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
            {
                services.AddServiceBus(configuration);
            }
            else
            {
                services.AddRabbitMQ(configuration);
            }

            services.AddTransient<TripCreatedIntegrationEventHandler>();
            services.AddTransient<TripFinishedIntegrationEventHandler>();
            services.AddTransient<TripUpdatedIntegrationEventHandler>();

            return services;
        }

        public static IServiceCollection AddSignalR(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.GetValue<bool>("IsDeployedOnCluster"))
            {
                services
                    .AddSignalR()
                    .AddRedis(configuration.GetConnectionString("SignalrBackPlane"));
            }
            else
            {
                services.AddSignalR();
            }

            return services;
        }

        public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var hcBuilder = services.AddHealthChecks();

            hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

            if (configuration.GetValue<bool>("IsDeployedOnCluster"))
            {
                hcBuilder
                    .AddRedis(
                        configuration.GetConnectionString("SignalrBackPlane"),
                        name: "SignalrBackPlane-check",
                        tags: new string[] { "backplane" });
            }

            if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
            {
                hcBuilder.AddAzureServiceBusTopic(configuration, "notifications-az-servicebus-check");
            }
            else
            {
                hcBuilder.AddRabbitMQ(configuration, "notifications-rabbitmqbus-check");
            }

            return services;
        }
    }
}
