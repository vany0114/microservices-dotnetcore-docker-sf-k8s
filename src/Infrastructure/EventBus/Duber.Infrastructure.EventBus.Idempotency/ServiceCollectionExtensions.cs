using Microsoft.Extensions.DependencyInjection;
using Duber.Infrastructure.EventBus.Abstractions;
using System.Linq;
using System;

namespace Duber.Infrastructure.EventBus.Idempotency
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterDefaultIdempotentHandler(this IServiceCollection services)
        {
            services.AddTransient(typeof(IIntegrationEventHandler<>), typeof(IdempotentIntegrationEventHandler<>));
            return services;
        }

        public static IServiceCollection RegisterIdempotentHandlers(this IServiceCollection services, Type assemblyType)
        {
            RegisterDefaultIdempotentHandler(services);

            assemblyType.Assembly
                .GetTypes()
                .Where(item => item.GetInterfaces().Where(i => i.IsGenericType).Any(i => i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)) && !item.IsAbstract && !item.IsInterface)
                .ToList()
                .ForEach(assignedTypes =>
                {
                    services.AddTransient(assignedTypes);
                });

            return services;
        }
    }
}
