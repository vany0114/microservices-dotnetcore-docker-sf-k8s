using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Duber.Invoice.API.Application.Validations;
using Duber.Invoice.API.Extensions;
using Duber.Invoice.API.Infrastructure.AutofacModules;
using Duber.Invoice.API.Infrastructure.Filters;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

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
                .AddServiceBroker(Configuration)
                .AddHealthChecks(Configuration)
                .AddCustomSwagger();

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

            app.UseCors("CorsPolicy")
                .UseRouting()
                .UseServiceBroker();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/readiness", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self")
                });
            });

            app.UseSwagger(x =>
                {
                    var reverseProxyPrefix = Configuration.GetValue<string>("ReverseProxyPrefix");
                    if (!string.IsNullOrEmpty(reverseProxyPrefix))
                    {
                        x.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Servers = new List<OpenApiServer>
                        {
                            new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}/{reverseProxyPrefix}" }
                        });
                    }
                })
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("./swagger/v1/swagger.json", "Duber.Invoice V1");
                    c.RoutePrefix = string.Empty;
                });
        }
    }
}
