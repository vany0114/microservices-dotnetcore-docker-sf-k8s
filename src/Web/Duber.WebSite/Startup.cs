using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Infrastructure.EventBus.RabbitMQ.IoC;
using Duber.Infrastructure.EventBus.ServiceBus.IoC;
using Duber.WebSite.Application.IntegrationEvents.Events;
using Duber.WebSite.Application.IntegrationEvents.Handlers;
using Duber.WebSite.Extensions;
using Duber.WebSite.Hubs;
using Duber.WebSite.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            services.AddMemoryCache()
                .Configure<FormOptions>(x => x.ValueCountLimit = 2048)
                .AddApplicationInsightsTelemetry(Configuration)
                .AddMvc();

            services.AddSignalR();

            services.Configure<TripApiSettings>(Configuration.GetSection("TripApiSettings"))
                .AddResilientStrategies(Configuration)
                .AddPersistenceAndRepositories(Configuration);

            // service bus configuration
            if (Configuration.GetValue<bool>("AzureServiceBusEnabled"))
            {
                services.AddServiceBus(Configuration);
            }
            else
            {
                services.AddRabbitMQ(Configuration);
            }
            
            RegisterEventBusHandlers(services);

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
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            ConfigureEventBusEvents(app);
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<TripHub>("/triphub");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void RegisterEventBusHandlers(IServiceCollection services)
        {
            services.AddTransient<TripCreatedIntegrationEventHandler>();
            services.AddTransient<TripUpdatedIntegrationEventHandler>();
            services.AddTransient<InvoiceCreatedIntegrationEventHandler>();
            services.AddTransient<InvoicePaidIntegrationEventHandler>();
        }

        private void ConfigureEventBusEvents(IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<TripCreatedIntegrationEvent, TripCreatedIntegrationEventHandler>();
            eventBus.Subscribe<TripUpdatedIntegrationEvent, TripUpdatedIntegrationEventHandler>();
            eventBus.Subscribe<InvoiceCreatedIntegrationEvent, InvoiceCreatedIntegrationEventHandler>();
            eventBus.Subscribe<InvoicePaidIntegrationEvent, InvoicePaidIntegrationEventHandler>();
        }
    }
}
