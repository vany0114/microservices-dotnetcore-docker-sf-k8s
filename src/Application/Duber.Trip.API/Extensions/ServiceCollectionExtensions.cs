using Duber.Domain.Trip.Commands;
using Duber.Trip.API.Application.DomainEventHandlers;
using Duber.Trip.API.Infrastructure.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Duber.Infrastructure.EventBus.Idempotency;
using Duber.Infrastructure.EventBus.RabbitMQ.IoC;
using Duber.Infrastructure.EventBus.ServiceBus.IoC;
using Kledex;
using Kledex.Commands;
using Kledex.Configuration;
using Kledex.Domain;
using Kledex.Events;
using Kledex.Extensions;
using Kledex.Queries;
using Kledex.Store.Cosmos.Mongo.Configuration;
using Microsoft.OpenApi.Models;
using Kledex.Store.Cosmos.Mongo.Extensions;
using MongoDB.Bson.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Duber.Trip.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCQRS(this IServiceCollection services, IConfiguration configuration)
        {
            // Kledex only needs a type per assembly, it automatically registers the rest of the commands, events, etc.
            services.Configure<MongoOptions>(configuration.GetSection("EventStoreConfiguration"));
            services.AddCustomKledex(options =>
                {
                    options.PublishEvents = true;
                    options.SaveCommandData = true;
                }, typeof(CreateTripCommand), typeof(TripCreatedDomainEventHandlerAsync))
                .AddCosmosMongoStore(__ => configuration.Get<MongoOptions>());
            services.AddTransient<IEventStoreRepository, EventStoreRepository>();

            return services;
        }

        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
        {
            // swagger configuration
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Duber.Trip HTTP API",
                    Version = "v1",
                    Description = "The Duber Trip Service HTTP API"
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            return services;
        }

        /// <summary>
        /// I had to override this method since I was using automapper to map out some commands and events and Kledex internally overrides those mappings.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static IKledexServiceBuilder AddCustomKledex(this IServiceCollection services, Action<MainOptions> setupAction, params Type[] types)
        {
            var typeList = types.ToList();
            typeList.Add(typeof(IDispatcher));

            services.Scan(s => s
                .FromAssembliesOf(typeList)
                .AddClasses()
                .AsImplementedInterfaces());

            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddCustomAutoMapper(typeList);
            services.Configure(setupAction);
            return new KledexServiceBuilder(services);
        }

        private static IServiceCollection AddCustomAutoMapper(this IServiceCollection services, List<Type> types)
        {
            var autoMapperConfig = new MapperConfiguration(cfg =>
            {
                foreach (var type in types)
                {
                    var typesToMap = type.Assembly.GetTypes()
                        .Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract && (
                                        typeof(ICommand).IsAssignableFrom(t) || 
                                        typeof(IEvent).IsAssignableFrom(t) || 
                                        typeof(IQuery<>).IsAssignableFrom(t)))
                        .ToList();

                    foreach (var typeToMap in typesToMap)
                    {
                        cfg.CreateMap(typeToMap, typeToMap);
                        cfg.AddMaps(types);
                    }
                }
            });

            services.AddSingleton(sp => autoMapperConfig.CreateMapper());
            return services;
        }

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

            return services;
        }

        public static IServiceCollection AddIdempotency(this IServiceCollection services)
        {
            services.AddTransient<IIdempotencyStoreProvider, IdempotencyStoreProvider>();
            services.RegisterIdempotentHandlers(typeof(TripUpdatedIdempotentEventHandler));

            BsonClassMap.RegisterClassMap<IdempotentMessage>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

            return services;
        }

        public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var hcBuilder = services.AddHealthChecks();

            hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

            hcBuilder
                .AddMongoDb(
                    configuration["EventStoreConfiguration:ConnectionString"],
                    mongoDatabaseName: string.Empty,
                    name: "TripDB-check",
                    tags: new string[] { "tripdb" });

            if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
            {
                hcBuilder.AddAzureServiceBusTopic(configuration, "trip-az-servicebus-check");
            }
            else
            {
                hcBuilder.AddRabbitMQ(configuration, "trip-rabbitmqbus-check");
            }

            return services;
        }
    }
}