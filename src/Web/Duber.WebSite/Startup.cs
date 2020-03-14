using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Duber.WebSite.Extensions;
using Duber.WebSite.Models;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
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

            services.Configure<TripApiSettings>(Configuration.GetSection("TripApiSettings"))
                .AddResilientStrategies(Configuration)
                .AddPersistenceAndRepositories(Configuration)
                .AddServiceBroker(Configuration)
                .AddHealthChecks(Configuration);

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

            app.UseStaticFiles()
                .UseRouting()
                .UseServiceBroker();

            app.UseHealthChecksUI(config => config.UIPath = "/hc-ui");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

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
        }
    }
}
