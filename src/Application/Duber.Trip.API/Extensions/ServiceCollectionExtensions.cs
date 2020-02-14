using Duber.Domain.Trip.Commands;
using Duber.Trip.API.Application.DomainEventHandlers;
using Duber.Trip.API.Infrastructure.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using Weapsy.Cqrs.EventStore.CosmosDB.MongoDB.Extensions;
using Weapsy.Cqrs.Extensions;

namespace Duber.Trip.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCQRS(this IServiceCollection services, IConfiguration configuration)
        {
            // Weapsy.CQRS only needs a type per assembly, it automatically registers the rest of the commands, events, etc.
            services.AddWeapsyCqrs(typeof(CreateTripCommand), typeof(TripCreatedDomainEventHandlerAsync));
            services.AddWeapsyCqrsEventStore(configuration);
            services.AddTransient<IEventStoreRepository, EventStoreRepository>();

            return services;
        }

        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
        {
            // swagger configuration
            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Title = "Duber.Trip HTTP API",
                    Version = "v1",
                    Description = "The Duber Trip Service HTTP API",
                    TermsOfService = "Terms Of Service"
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            return services;
        }
    }
}