using System;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Duber.Domain.Trip.Commands;
using Duber.Infrastructure.EventBus;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Infrastructure.EventBus.RabbitMQ;
using Duber.Infrastructure.EventBus.ServiceBus;
using Duber.Trip.API.Application.DomainEventHandlers;
using Duber.Trip.API.Application.Validations;
using Duber.Trip.API.Infrastructure.Filters;
using Duber.Trip.API.Infrastructure.Repository;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Weapsy.Cqrs.EventStore.CosmosDB.MongoDB.Extensions;
using Weapsy.Cqrs.Extensions;
// ReSharper disable InconsistentNaming

namespace Duber.Trip.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper();
            services.AddMvc(options =>
                {
                    options.Filters.Add(typeof(HttpGlobalExceptionFilter));
                    options.Filters.Add(typeof(ValidatorActionFilter));
                })
                .AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<UpdateTripCommandValidator>());

            services.AddOptions();

            // Event store configuration:
            // Weapsy.CQRS only needs a type per assembly, it automatically registers the rest of the commands, events, etc.
            services.AddWeapsyCqrs(typeof(CreateTripCommand), typeof(TripCreatedDomainEventHandlerAsync));
            services.AddWeapsyCqrsEventStore(Configuration);
            services.AddTransient<IEventStoreRepository, EventStoreRepository>();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

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

            // service bus configuration
            if (Configuration.GetValue<bool>("AzureServiceBusEnabled"))
            {
                services.AddSingleton<IServiceBusPersisterConnection>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<DefaultServiceBusPersisterConnection>>();
                    var serviceBusConnectionString = Configuration["EventBusConnection"];
                    var serviceBusConnection = new ServiceBusConnectionStringBuilder(serviceBusConnectionString);

                    return new DefaultServiceBusPersisterConnection(serviceBusConnection, logger);
                });
            }
            else
            {
                services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
                    var factory = new ConnectionFactory()
                    {
                        HostName = Configuration["EventBusConnection"]
                    };

                    var retryCount = 5;
                    if (!string.IsNullOrEmpty(Configuration["EventBusRetryCount"]))
                    {
                        retryCount = int.Parse(Configuration["EventBusRetryCount"]);
                    }

                    return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
                });
            }
            
            RegisterEventBus(services);

            var container = new ContainerBuilder();
            container.Populate(services);
            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddAzureWebAppDiagnostics();
            loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Trace);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");
            app.UseMvc();

            app.UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Duber.Trip V1");
                    c.RoutePrefix = string.Empty;
                });
        }

        private void RegisterEventBus(IServiceCollection services)
        {
            var retryCount = 5;
            if (!string.IsNullOrEmpty(Configuration["EventBusRetryCount"]))
            {
                retryCount = int.Parse(Configuration["EventBusRetryCount"]);
            }

            if (Configuration.GetValue<bool>("AzureServiceBusEnabled"))
            {
                services.AddSingleton<IEventBus, EventBusServiceBus>(sp =>
                {
                    var serviceBusPersisterConnection = sp.GetRequiredService<IServiceBusPersisterConnection>();
                    var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                    var logger = sp.GetRequiredService<ILogger<EventBusServiceBus>>();
                    var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
                    var subscriptionClientName = Configuration["SubscriptionClientName"];

                    return new EventBusServiceBus(serviceBusPersisterConnection, logger,
                        eventBusSubcriptionsManager, subscriptionClientName, iLifetimeScope, retryCount);
                });
            }
            else
            {
                services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
                {
                    var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                    var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                    var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                    var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                    return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager, retryCount);
                });
            }

            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
        }
    }
}
