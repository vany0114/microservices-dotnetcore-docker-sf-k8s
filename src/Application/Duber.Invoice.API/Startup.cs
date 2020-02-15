using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Infrastructure.EventBus.RabbitMQ.IoC;
using Duber.Infrastructure.EventBus.ServiceBus.IoC;
using Duber.Invoice.API.Application.IntegrationEvents.Events;
using Duber.Invoice.API.Application.IntegrationEvents.Hnadlers;
using Duber.Invoice.API.Application.Validations;
using Duber.Invoice.API.Extensions;
using Duber.Invoice.API.Infrastructure.AutofacModules;
using Duber.Invoice.API.Infrastructure.Filters;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            services
                .AddAutoMapper()
                .AddApplicationInsightsTelemetry(Configuration)
                .AddControllers(options =>
                {
                    options.Filters.Add(typeof(HttpGlobalExceptionFilter));
                    options.Filters.Add(typeof(ValidatorActionFilter));
                })
                .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
                .AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<CreateInvoiceRequestValidator>());

            services.AddOptions()
                .AddCors(options =>
                {
                    // be careful with this policy, in production you should only add authorized origins, methods, etc.
                    options.AddPolicy("CorsPolicy",
                        builder => builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader());
                });

            services.AddResilientStrategies(Configuration)
                .AddPersistenceAndRepository(Configuration)
                .AddPaymentService(Configuration)
                .AddCustomSwagger();

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

            //configure autofac
            var container = new ContainerBuilder();
            container.Populate(services);
            container.RegisterModule(new MediatorModule());

            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
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
