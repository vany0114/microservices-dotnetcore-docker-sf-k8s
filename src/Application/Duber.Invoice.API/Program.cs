using Duber.Domain.Invoice.Persistence;
using Duber.Infrastructure.WebHost;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
#pragma warning disable 618

namespace Duber.Invoice.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args)
                .MigrateDbContext<InvoiceMigrationContext>((_, __) => { })
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
