using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Duber.Infrastructure.Resilience.SqlServer
{
    public class ResilientSqlExecutorFactory : ISqlExecutor
    {
        private readonly ILogger<ResilientExecutor<ISqlExecutor>> _logger;
        private readonly int _retryCount;
        private readonly int _exceptionsAllowedBeforeBreaking;

        public ResilientSqlExecutorFactory(ILogger<ResilientExecutor<ISqlExecutor>> logger, int retryCount, int exceptionsAllowedBeforeBreaking)
        {
            _logger = logger;
            _retryCount = retryCount;
            _exceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking;
        }

        public ResilientExecutor<ISqlExecutor> CreateResilientSqlClient()
            => new ResilientExecutor<ISqlExecutor>(CreatePolicies());

        /// <summary>
        /// Consider include in your policies all exceptions as you needed.
        /// https://docs.microsoft.com/en-us/azure/sql-database/sql-database-develop-error-messages
        /// </summary>
        private AsyncPolicy[] CreatePolicies()
            => new AsyncPolicy[]
            {
                Policy.Handle<SqlException>(ex => ex.Number == 40613)
                    .Or<SqlException>(ex => ex.Number == 40197)
                    .Or<SqlException>(ex => ex.Number == 40501)
                    .Or<SqlException>(ex => ex.Number == 49918)
                    .WaitAndRetryAsync(
                        // number of retries
                        _retryCount,
                        // exponential backofff
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        // on retry
                        (exception, timeSpan, retryCount, context) =>
                        {
                            var msg = $"Retry {retryCount} implemented with Polly's RetryPolicy " +
                                      $"of {context.PolicyKey} " +
                                      $"at {context.OperationKey}, " +
                                      $"due to: {exception}.";
                            _logger.LogWarning(msg);
                            _logger.LogDebug(msg);
                        }),
                Policy.Handle<SqlException>()
                    .CircuitBreakerAsync( 
                        // number of exceptions before breaking circuit
                        _exceptionsAllowedBeforeBreaking,
                        // time circuit opened before retry
                        TimeSpan.FromMinutes(1),
                        (exception, duration) =>
                        {
                            // on circuit opened
                            _logger.LogTrace("Circuit breaker opened");
                        },
                        () =>
                        {
                            // on circuit closed
                            _logger.LogTrace("Circuit breaker reset");
                        })
            };
    }
}
