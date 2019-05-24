using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Duber.Domain.Driver.Model;
using Microsoft.Extensions.Logging;
using Polly;

namespace Duber.Domain.Driver.Persistence
{
    public class DriverContextSeed
    {
        public async Task SeedAsync(DriverContext context, ILogger<DriverContextSeed> logger)
        {
            var policy = CreatePolicy(logger, nameof(DriverContextSeed));

            await policy.ExecuteAsync(async () =>
            {
                using (context)
                {
                    if (!context.DriverStatuses.Any())
                    {
                        context.DriverStatuses.AddRange(GetPreconfiguredDriverStatuses());
                        await context.SaveChangesAsync();
                    }

                    if (!context.VehicleTypes.Any())
                    {
                        context.VehicleTypes.AddRange(GetPreconfiguredVehicleTypes());
                        await context.SaveChangesAsync();
                    }

                    if (!context.Drivers.Any())
                    {
                        context.Drivers.AddRange(GetPreconfiguredDrivers());
                        await context.SaveChangesAsync();
                    }
                }
            });
        }

        private static IEnumerable<Model.Driver> GetPreconfiguredDrivers() => new List<Model.Driver>
        {
            new Model.Driver("The Transporter", "Jason.Statham@hotmail.com", 3, "YUI 789", "Audi", "2002", VehicleType.Car),
            new Model.Driver("Toreto", "toreto@gmail.com", 4, "BNE 456", "Ford", "2009", VehicleType.Car),
            new Model.Driver("Jackie Chan", "jackie@gmail.com", 5, "EQM 197", "Cat", "2015", VehicleType.TuckTuck),
            new Model.Driver("Robert De Niro", "deniro@hotmail.com", 5, "GXU 713", "BMW", "2013", VehicleType.Car),
            new Model.Driver("Valentino Rossi", "vrossi@hotmail.com", 4, "GPX 570", "Yamaha", "2017", VehicleType.Bike)
        };

        private static IEnumerable<VehicleType> GetPreconfiguredVehicleTypes() => new List<VehicleType>
        {
            VehicleType.Car,
            VehicleType.Bike,
            VehicleType.TuckTuck
        };

        private static IEnumerable<DriverStatus> GetPreconfiguredDriverStatuses() => new List<DriverStatus>
        {
            DriverStatus.Available,
            DriverStatus.Busy,
            DriverStatus.Inactive,
            DriverStatus.Active
        };

        private static AsyncPolicy CreatePolicy(ILogger<DriverContextSeed> logger, string prefix, int retries = 3)
        {
            return Policy.Handle<SqlException>().
                WaitAndRetryAsync(
                    retryCount: retries,
                    sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                    onRetry: (exception, timeSpan, retry, ctx) =>
                    {
                        logger.LogTrace($"[{prefix}] Exception {exception.GetType().Name} with message ${exception.Message} detected on attempt {retry} of {retries}");
                    }
                );
        }
    }
}
