using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Duber.Infrastructure.EventBus.RabbitMQ.IoC;
using Duber.Infrastructure.EventBus.ServiceBus.IoC;
using Duber.Trip.API.Application.Mapping;
using Duber.Trip.API.Application.Validations;
using Duber.Trip.API.Extensions;
using Duber.Trip.API.Infrastructure.Filters;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            services.AddApplicationInsightsTelemetry(Configuration)
                .AddControllers(options =>
                {
                    options.Filters.Add(typeof(HttpGlobalExceptionFilter));
                    options.Filters.Add(typeof(ValidatorActionFilter));
                })
                .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
                .AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<UpdateTripCommandValidator>());

            services.AddCQRS(Configuration);
            services.AddOptions()
                .AddCors(options =>
                {
                    // be careful with this policy, in production you should only add authorized origins, methods, etc.
                    options.AddPolicy("CorsPolicy",
                        builder => builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader());
                });

            services.AddCustomSwagger();

            // service bus configuration
            if (Configuration.GetValue<bool>("AzureServiceBusEnabled"))
            {
                services.AddServiceBus(Configuration);
            }
            else
            {
                services.AddRabbitMQ(Configuration);
            }

            var container = new ContainerBuilder();
            container.Populate(services);
            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseCors("CorsPolicy");
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Duber.Trip V1");
                    c.RoutePrefix = string.Empty;
                });
        }
    }
}
