using System;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Duber.Domain.Driver.Persistence;
using Duber.Domain.Driver.Repository;
using Duber.Domain.User.Persistence;
using Duber.Domain.User.Repository;
using Duber.Infrastructure.EventBus;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Infrastructure.EventBus.RabbitMQ;
using Duber.Infrastructure.EventBus.ServiceBus;
using Duber.Infrastructure.Resilience.Http;
using Duber.WebSite.Application.IntegrationEvents.Events;
using Duber.WebSite.Application.IntegrationEvents.Handlers;
using Duber.WebSite.Hubs;
using Duber.WebSite.Infrastructure.Persistence;
using Duber.WebSite.Infrastructure.Repository;
using Duber.WebSite.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
// ReSharper disable InconsistentNaming
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable AssignNullToNotNullAttribute

namespace Duber.WebSite
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
            services.AddMemoryCache();
            services.Configure<FormOptions>(x => x.ValueCountLimit = 2048);
            services.AddMvc();
            services.AddSignalR();

            services.Configure<TripApiSettings>(Configuration.GetSection("TripApiSettings"));

            services.AddDbContext<UserContext>(options =>
            {
                options.UseSqlServer(
                    Configuration["ConnectionString"],
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(UserContext).GetTypeInfo().Assembly.GetName().Name);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    });
            });

            services.AddDbContext<DriverContext>(options =>
            {
                options.UseSqlServer(
                    Configuration["ConnectionString"],
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(DriverContext).GetTypeInfo().Assembly.GetName().Name);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    });
            });

            services.AddDbContext<ReportingContext>(options =>
            {
                options.UseSqlServer(
                    Configuration["ConnectionString"],
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(ReportingContext).GetTypeInfo().Assembly.GetName().Name);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    });
            });

            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IDriverRepository, DriverRepository>();
            services.AddTransient<IReportingRepository, ReportingRepository>();

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

            var container = new ContainerBuilder();
            container.Populate(services);
            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseSignalR(routes =>
            {
                routes.MapHub<TripHub>("/triphub");
            });

            ConfigureEventBus(app);
            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
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
            services.AddTransient<TripCreatedIntegrationEventHandler>();
            services.AddTransient<TripUpdatedIntegrationEventHandler>();
            services.AddTransient<InvoiceCreatedIntegrationEventHandler>();
            services.AddTransient<InvoicePaidIntegrationEventHandler>();
        }

        private void ConfigureEventBus(IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<TripCreatedIntegrationEvent, TripCreatedIntegrationEventHandler>();
            eventBus.Subscribe<TripUpdatedIntegrationEvent, TripUpdatedIntegrationEventHandler>();
            eventBus.Subscribe<InvoiceCreatedIntegrationEvent, InvoiceCreatedIntegrationEventHandler>();
            eventBus.Subscribe<InvoicePaidIntegrationEvent, InvoicePaidIntegrationEventHandler>();
        }
    }
}
