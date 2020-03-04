using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Infrastructure.EventBus.RabbitMQ.IoC;
using Duber.Infrastructure.EventBus.ServiceBus.IoC;
using Duber.Trip.Notifications.Application.IntegrationEvents.Events;
using Duber.Trip.Notifications.Application.IntegrationEvents.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Duber.Trip.Notifications
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
            services
                .AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy",
                        builder => builder
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .SetIsOriginAllowed((host) => true)
                            .AllowCredentials());
                });

            if (Configuration.GetValue<bool>("IsDeployedOnCluster"))
            {
                services
                    .AddSignalR()
                    .AddRedis(Configuration.GetConnectionString("SignalrBackPlane"));
            }
            else
            {
                services.AddSignalR();
            }

            // service bus configuration
            if (Configuration.GetValue<bool>("AzureServiceBusEnabled"))
            {
                services.AddServiceBus(Configuration);
            }
            else
            {
                services.AddRabbitMQ(Configuration);
            }

            RegisterEventBus(services);
            
            var container = new ContainerBuilder();
            container.Populate(services);

            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<NotificationsHub>("/hub/notification",
                    options => options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransports.All);
            });
            
            ConfigureEventBus(app);
        }

        private void RegisterEventBus(IServiceCollection services)
        {
            services.AddTransient<TripCreatedIntegrationEventHandler>();
            services.AddTransient<TripFinishedIntegrationEventHandler>();
            services.AddTransient<TripUpdatedIntegrationEventHandler>();
        }

        private void ConfigureEventBus(IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<TripCreatedIntegrationEvent, TripCreatedIntegrationEventHandler>();
            eventBus.Subscribe<TripFinishedIntegrationEvent, TripFinishedIntegrationEventHandler>();
            eventBus.Subscribe<TripUpdatedIntegrationEvent, TripUpdatedIntegrationEventHandler>();
        }
    }
}
