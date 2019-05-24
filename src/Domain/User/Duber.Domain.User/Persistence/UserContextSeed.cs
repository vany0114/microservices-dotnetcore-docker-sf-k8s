using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Duber.Domain.SharedKernel.Model;
using Microsoft.Extensions.Logging;
using Polly;

namespace Duber.Domain.User.Persistence
{
    public class UserContextSeed
    {
        public async Task SeedAsync(UserContext context, ILogger<UserContextSeed> logger)
        {
            var policy = CreatePolicy(logger, nameof(UserContextSeed));

            await policy.ExecuteAsync(async () =>
            {
                using (context)
                {
                    if (!context.PaymentMethods.Any())
                    {
                        context.PaymentMethods.AddRange(GetPreconfiguredPaymentMethods());
                        await context.SaveChangesAsync();
                    }

                    if (!context.Users.Any())
                    {
                        context.Users.AddRange(GetPreconfiguredUsers());
                        await context.SaveChangesAsync();
                    }
                }
            });
        }

        private static IEnumerable<Model.User> GetPreconfiguredUsers() => new List<Model.User>
        {
            new Model.User("James Hedfield", "jamesh@metallica.com", 5, PaymentMethod.CreditCard, "1234567890"),
            new Model.User("Rob Haldford", "robh@judaspriest.com", 5, PaymentMethod.CreditCard, "7894561230"),
            new Model.User("Jimi Hendrix", "heyjoe@latinchat.com", 5, PaymentMethod.Cash, "7894561230"),
            new Model.User("Steve Vai", "stevevay@hotmail.com", 5, PaymentMethod.CreditCard, "4567891230"),
            new Model.User("Joe Satriani", "jsatriani@gmail.com", 5, PaymentMethod.Cash, "7418529630"),
        };

        private static IEnumerable<PaymentMethod> GetPreconfiguredPaymentMethods() => new List<PaymentMethod>()
        {
            PaymentMethod.CreditCard,
            PaymentMethod.Cash,
        };

        private static AsyncPolicy CreatePolicy(ILogger<UserContextSeed> logger, string prefix, int retries = 3)
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
