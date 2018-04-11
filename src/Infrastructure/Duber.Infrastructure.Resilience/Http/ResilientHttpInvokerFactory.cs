using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Polly;

namespace Duber.Infrastructure.Resilience.Http
{
    public class ResilientHttpInvokerFactory
    {
        private readonly ILogger<ResilientHttpInvoker> _logger;
        private readonly int _retryCount;
        private readonly int _exceptionsAllowedBeforeBreaking;

        public ResilientHttpInvokerFactory(ILogger<ResilientHttpInvoker> logger, int retryCount, int exceptionsAllowedBeforeBreaking)
        {
            _logger = logger;
            _retryCount = retryCount;
            _exceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking;
        }

        public ResilientHttpInvoker CreateResilientHttpClient()
            => new ResilientHttpInvoker(CreatePolicies());

        private Policy[] CreatePolicies()
            => new Policy[]
            {
                Policy.Handle<HttpRequestException>()
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
                Policy.Handle<HttpRequestException>()
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