using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duber.Domain.Trip.Commands;
using Duber.Domain.Trip.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weapsy.Cqrs.EventStore.CosmosDB.MongoDB.Extensions;
using Weapsy.Cqrs.Extensions;

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
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddOptions();

            // Event store configuration:
            // Weapsy.CQRS only needs a type per assembly, it automatically registers the rest of the commands, events, etc.
            services.AddWeapsyCqrs(typeof(CreateTrip));
            services.AddWeapsyCqrsEventStore(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
