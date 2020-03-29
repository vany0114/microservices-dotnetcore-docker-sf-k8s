using Autofac;
using Duber.Infrastructure.EventBus.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Duber.Infrastructure.EventBus.RabbitMQ.IoC
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
        {
            var retryCount = 5;
            if (!string.IsNullOrEmpty(configuration["EventBusRetryCount"]))
            {
                retryCount = int.Parse(configuration["EventBusRetryCount"]);
            }

            var connectionFactory = new ConnectionFactory()
            {
                HostName = configuration["EventBusConnection"],
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true
            };

            if (!string.IsNullOrEmpty(configuration["EventBusUserName"]))
            {
                connectionFactory.UserName = configuration["EventBusUserName"];
            }

            if (!string.IsNullOrEmpty(configuration["EventBusPassword"]))
            {
                connectionFactory.Password = configuration["EventBusPassword"];
            }

            services.AddSingleton<IConnectionFactory>(connectionFactory);
            services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
                var factory = sp.GetRequiredService<IConnectionFactory>();

                return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
            });

            services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
                var subscriptionClientName = configuration["SubscriptionClientName"];

                return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager, subscriptionClientName, retryCount);
            });

            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
            return services;
        }

        public static IHealthChecksBuilder AddRabbitMQ(this IHealthChecksBuilder healthChecksBuilder, IConfiguration configuration, string name = "rabbitmqbus-check")
        {
            healthChecksBuilder
                .AddRabbitMQ((sp) => sp.GetRequiredService<IConnectionFactory>().CreateConnection(),
                    name: name,
                    tags: new string[] { "rabbitmqbus" });

            return healthChecksBuilder;
        }
    }
}