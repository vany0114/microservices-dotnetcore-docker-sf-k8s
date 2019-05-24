using System;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Duber.Domain.ACL.Adapters;
using Duber.Domain.ACL.Contracts;
using Duber.Domain.Invoice.Persistence;
using Duber.Domain.Invoice.Repository;
using Duber.Domain.Invoice.Services;
using Duber.Infrastructure.EventBus;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Infrastructure.EventBus.RabbitMQ;
using Duber.Infrastructure.EventBus.ServiceBus;
using Duber.Infrastructure.Resilience;
using Duber.Infrastructure.Resilience.Http;
using Duber.Infrastructure.Resilience.SqlServer;
using Duber.Invoice.API.Application.IntegrationEvents.Events;
using Duber.Invoice.API.Application.IntegrationEvents.Hnadlers;
using Duber.Invoice.API.Application.Validations;
using Duber.Invoice.API.Infrastructure.AutofacModules;
using Duber.Invoice.API.Infrastructure.Filters;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
// ReSharper disable InconsistentNaming
// ReSharper disable AssignNullToNotNullAttribute
#pragma warning disable 618

namespace Duber.Invoice.API
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
                .AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<CreateInvoiceRequestValidator>());

            services.AddOptions();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            // just to perform the migrations
            services.AddDbContext<InvoiceMigrationContext>(options =>
            {
                options.UseSqlServer(
                    Configuration["ConnectionString"],
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(InvoiceMigrationContext).GetTypeInfo().Assembly.GetName().Name);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    });
            });

            // swagger configuration
            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Title = "Duber.Invoice HTTP API",
                    Version = "v1",
                    Description = "The Duber Invoice Service HTTP API",
                    TermsOfService = "Terms Of Service"
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            // Resilient SQL Executor configuration.
            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ResilientExecutor<ISqlExecutor>>>();

                var retryCount = 5;
                if (!string.IsNullOrEmpty(Configuration["SqlClientRetryCount"]))
                {
                    retryCount = int.Parse(Configuration["SqlClientRetryCount"]);
                }

                var exceptionsAllowedBeforeBreaking = 4;
                if (!string.IsNullOrEmpty(Configuration["SqlClientExceptionsAllowedBeforeBreaking"]))
                {
                    exceptionsAllowedBeforeBreaking = int.Parse(Configuration["SqlClientExceptionsAllowedBeforeBreaking"]);
                }

                return new ResilientSqlExecutorFactory(logger, exceptionsAllowedBeforeBreaking, retryCount);
            });
            services.AddTransient(sp => sp.GetService<ResilientSqlExecutorFactory>().CreateResilientSqlClient());

            // Invoice repository and context configuration
            services.AddTransient<IInvoiceContext, InvoiceContext>(provider =>
            {
                var mediator = provider.GetService<IMediator>();
                var sqlExecutor = provider.GetService<ResilientExecutor<ISqlExecutor>>();
                var connectionString = Configuration["ConnectionString"];
                return new InvoiceContext(connectionString, mediator, sqlExecutor);
            });
            services.AddTransient<IInvoiceRepository, InvoiceRepository>();

            // payment service configuration
            services.AddTransient<IPaymentService, PaymentService>();
            services.AddTransient<IPaymentServiceAdapter, PaymentServiceAdapter>(provider =>
            {
                var httpInvoker = provider.GetRequiredService<ResilientHttpInvoker>();
                var paymentServiceBaseUrl = Configuration["PaymentServiceBaseUrl"];
                return new PaymentServiceAdapter(httpInvoker, paymentServiceBaseUrl);
            });

            // Resilient Http Invoker onfiguration.
            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ResilientHttpInvoker>>();

                var retryCount = 5;
                if (!string.IsNullOrEmpty(Configuration["HttpClientRetryCount"]))
                {
                    retryCount = int.Parse(Configuration["HttpClientRetryCount"]);
                }

                var exceptionsAllowedBeforeBreaking = 4;
                if (!string.IsNullOrEmpty(Configuration["HttpClientExceptionsAllowedBeforeBreaking"]))
                {
                    exceptionsAllowedBeforeBreaking = int.Parse(Configuration["HttpClientExceptionsAllowedBeforeBreaking"]);
                }

                return new ResilientHttpInvokerFactory(logger, exceptionsAllowedBeforeBreaking, retryCount);
            });
            services.AddTransient(sp => sp.GetService<ResilientHttpInvokerFactory>().CreateResilientHttpClient());

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

            //configure autofac
            var container = new ContainerBuilder();
            container.Populate(services);
            container.RegisterModule(new MediatorModule());

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
            ConfigureEventBus(app);

            app.UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Duber.Invoice V1");
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
            services.AddTransient<TripCancelledIntegrationEventHandler>();
            services.AddTransient<TripFinishedIntegrationEventHandler>();
        }

        private void ConfigureEventBus(IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<TripCancelledIntegrationEvent, TripCancelledIntegrationEventHandler>();
            eventBus.Subscribe<TripFinishedIntegrationEvent, TripFinishedIntegrationEventHandler>();
        }
    }
}
