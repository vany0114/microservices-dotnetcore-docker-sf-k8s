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
using Duber.WebSite.Application.IntegrationEvents.Events;
using Duber.WebSite.Application.IntegrationEvents.Handlers;
using Duber.WebSite.Infrastructure.Persistence;
using Duber.WebSite.Infrastructure.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            services.AddMvc();

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

            // service bus configuration
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
            services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                var retryCount = 5;
                if (!string.IsNullOrEmpty(Configuration["EventBusRetryCount"]))
                {
                    retryCount = int.Parse(Configuration["EventBusRetryCount"]);
                }

                return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager, retryCount);
            });

            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
            services.AddTransient<TripCreatedIntegrationEventHandler>();
            services.AddTransient<TripUpdatedIntegrationEventHandler>();
            services.AddTransient<InvoiceCreatedIntegrationEventHandler>();
            services.AddTransient<InvoicePaidIntegrationEventHandler>();
        }

        protected virtual void ConfigureEventBus(IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<TripCreatedIntegrationEvent, TripCreatedIntegrationEventHandler>();
            eventBus.Subscribe<TripUpdatedIntegrationEvent, TripUpdatedIntegrationEventHandler>();
            eventBus.Subscribe<InvoiceCreatedIntegrationEvent, InvoiceCreatedIntegrationEventHandler>();
            eventBus.Subscribe<InvoicePaidIntegrationEvent, InvoicePaidIntegrationEventHandler>();
        }
    }
}
