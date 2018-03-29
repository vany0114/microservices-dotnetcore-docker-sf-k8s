using Duber.Domain.Driver.Persistence;
using Duber.Domain.User.Persistence;
using Duber.Infrastructure.WebHost;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Duber.WebSite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args)
                .MigrateDbContext<UserContext>((context, services) =>
                {
                    var logger = services.GetService<ILogger<UserContextSeed>>();
                    new UserContextSeed()
                        .SeedAsync(context, logger)
                        .Wait();
                })
                .MigrateDbContext<DriverContext>((context, services) =>
                {
                    var logger = services.GetService<ILogger<DriverContextSeed>>();
                    new DriverContextSeed()
                        .SeedAsync(context, logger)
                        .Wait();
                })
                .Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
