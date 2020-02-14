using Duber.Domain.Invoice.Persistence;
using Duber.Infrastructure.WebHost;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Duber.Invoice.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args)
#pragma warning disable CS0618 // Type or member is obsolete
                .MigrateDbContext<InvoiceMigrationContext>((_, __) => { })
#pragma warning restore CS0618 // Type or member is obsolete
                .Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, builder) =>
                {
                    builder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    builder.AddConsole();
                    builder.AddDebug();
                    builder.AddApplicationInsights();
                })
                .Build();
    }
}
