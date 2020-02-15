using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Duber.Infrastructure.Resilience.Sql.Internals;
using log4net;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Duber.Infrastructure.Resilience.Sql.Policies
{
    /// <summary>
    /// Sql error codes for Azure Sql.
    /// https://docs.microsoft.com/en-us/azure/sql-database/sql-database-develop-error-messages
    /// </summary>
    internal class AsyncPolicies
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AsyncPolicies));

        private static readonly int[] SqlTransientErrors =
        {
            (int)SqlHandledExceptions.DatabaseNotCurrentlyAvailable,
            (int)SqlHandledExceptions.ErrorProcessingRequest,
            (int)SqlHandledExceptions.ServiceCurrentlyBusy,
            (int)SqlHandledExceptions.NotEnoughResources
        };

        private static readonly int[] SqlTransactionErrors =
        {
            (int)SqlHandledExceptions.SessionTerminatedLongTransaction,
            (int)SqlHandledExceptions.SessionTerminatedToManyLocks
        };

        /// <summary>
        /// Gets a Retry policy for the most common transient error in Azure Sql.
        /// </summary>
        public static IAsyncPolicy GetCommonTransientErrorsPolicies(int retryCount) =>
            Policy
                .Handle<SqlException>(ex => SqlTransientErrors.Contains(ex.Number))
                .WaitAndRetryAsync(
                    // number of retries
                    retryCount,
                    // exponential back-off
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    // on retry
                    (exception, timeSpan, retries, context) =>
                    {
                        if (retryCount != retries)
                            return;

                        // only log if the final retry fails
                        var msg = $"#Polly #WaitAndRetryAsync Retry {retries}" +
                                  $"of {context.PolicyKey} " +
                                  $"due to: {exception}.";
                        Log.Error(msg, exception);
                    })
                .WithPolicyKey(PolicyKeys.SqlCommonTransientErrorsAsyncPolicy);

        /// <summary>
        /// Gets a Retry policy for the most common transaction errors in Azure Sql.
        /// </summary>
        public static IAsyncPolicy GetTransactionPolicy(int retryCount) =>
            Policy
                .Handle<SqlException>(ex => SqlTransactionErrors.Contains(ex.Number))
                .WaitAndRetryAsync(
                    // number of retries
                    retryCount,
                    // exponential back-off
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    // on retry
                    (exception, timeSpan, retries, context) =>
                    {
                        if (retryCount != retries)
                            return;

                        // only log if the final retry fails
                        var msg = $"#Polly #WaitAndRetryAsync Retry {retries}" +
                                  $"of {context.PolicyKey} " +
                                  $"due to: {exception}.";
                        Log.Error(msg, exception);
                    })
                .WithPolicyKey(PolicyKeys.SqlTransactionAsyncPolicy);

        /// <summary>
        /// Gets the circuit breaker policies for Transient and Transaction errors.
        /// </summary>
        /// <remarks>
        /// The circuit-breaker will break after N consecutive actions executed through the policy have thrown 'a' handled exception - any of the exceptions handled by the policy.
        /// So, we might want the circuit ONLY breaks after throws consecutively the SAME exception N times, that's why I'm defining separate circuit-breaker policies.
        /// </remarks>
        /// <example>
        /// Circuit broken = 3 consecutive SqlException(40613), and not, circuit broken = 3 SqlException(40613 or 40197 or 40501...)
        /// https://github.com/App-vNext/Polly/issues/490
        /// </example>
        public static IAsyncPolicy[] GetCircuitBreakerPolicies(int exceptionsAllowedBeforeBreaking)
            => new IAsyncPolicy[]
            {
                Policy
                    .Handle<SqlException>(ex => ex.Number == (int)SqlHandledExceptions.DatabaseNotCurrentlyAvailable)
                    .CircuitBreakerAsync(
                        // number of exceptions before breaking circuit
                        exceptionsAllowedBeforeBreaking,
                        // time circuit opened before retry
                        TimeSpan.FromSeconds(30),
                        OnBreak,
                        OnReset,
                        OnHalfOpen)
                    .WithPolicyKey($"F1.{PolicyKeys.SqlCircuitBreakerAsyncPolicy}"),
                Policy
                    .Handle<SqlException>(ex => ex.Number == (int)SqlHandledExceptions.ErrorProcessingRequest)
                    .CircuitBreakerAsync(
                        // number of exceptions before breaking circuit
                        exceptionsAllowedBeforeBreaking,
                        // time circuit opened before retry
                        TimeSpan.FromSeconds(30),
                        OnBreak,
                        OnReset,
                        OnHalfOpen)
                    .WithPolicyKey($"F2.{PolicyKeys.SqlCircuitBreakerAsyncPolicy}"),
                Policy
                    .Handle<SqlException>(ex => ex.Number == (int)SqlHandledExceptions.ServiceCurrentlyBusy)
                    .CircuitBreakerAsync(
                        // number of exceptions before breaking circuit
                        exceptionsAllowedBeforeBreaking,
                        // time circuit opened before retry
                        TimeSpan.FromSeconds(30),
                        OnBreak,
                        OnReset,
                        OnHalfOpen)
                    .WithPolicyKey($"F3.{PolicyKeys.SqlCircuitBreakerAsyncPolicy}"),
                Policy
                    .Handle<SqlException>(ex => ex.Number == (int)SqlHandledExceptions.NotEnoughResources)
                    .CircuitBreakerAsync(
                        // number of exceptions before breaking circuit
                        exceptionsAllowedBeforeBreaking,
                        // time circuit opened before retry
                        TimeSpan.FromSeconds(30),
                        OnBreak,
                        OnReset,
                        OnHalfOpen)
                    .WithPolicyKey($"F4.{PolicyKeys.SqlCircuitBreakerAsyncPolicy}"),
                Policy
                    .Handle<SqlException>(ex => ex.Number == (int)SqlHandledExceptions.SessionTerminatedLongTransaction)
                    .CircuitBreakerAsync(
                        // number of exceptions before breaking circuit
                        exceptionsAllowedBeforeBreaking,
                        // time circuit opened before retry
                        TimeSpan.FromSeconds(30),
                        OnBreak,
                        OnReset,
                        OnHalfOpen)
                    .WithPolicyKey($"F5.{PolicyKeys.SqlCircuitBreakerAsyncPolicy}"),
                Policy
                    .Handle<SqlException>(ex => ex.Number == (int)SqlHandledExceptions.SessionTerminatedToManyLocks)
                    .CircuitBreakerAsync(
                        // number of exceptions before breaking circuit
                        exceptionsAllowedBeforeBreaking,
                        // time circuit opened before retry
                        TimeSpan.FromSeconds(30),
                        OnBreak,
                        OnReset,
                        OnHalfOpen)
                    .WithPolicyKey($"F6.{PolicyKeys.SqlCircuitBreakerAsyncPolicy}")
            };

        /// <summary>
        /// Gets a pessimistic timeout policy in order to cancel the process which doesn't even honor cancellation.
        /// https://github.com/App-vNext/Polly/wiki/Timeout#pessimistic-timeout
        /// </summary>
        public static IAsyncPolicy GetTimeOutPolicy(TimeSpan timeout, string policyName) =>
            Policy
                .TimeoutAsync(
                    timeout,
                    TimeoutStrategy.Pessimistic)
                .WithPolicyKey(policyName);

        /// <summary>
        /// Handles a fallback policy for SqlException, TimeoutRejectedException and BrokenCircuitException exceptions. It means, if the process fails for one of those reasons, the fallback method is executed instead.
        /// </summary>
        public static IAsyncPolicy GetFallbackPolicy<T>(Func<Task<T>> action) =>
            Policy
                .Handle<SqlException>(ex => SqlTransientErrors.Contains(ex.Number))
                .Or<SqlException>(ex => SqlTransactionErrors.Contains(ex.Number))
                .Or<TimeoutRejectedException>()
                .Or<BrokenCircuitException>()
                .FallbackAsync(cancellationToken => action(),
                    ex =>
                    {
                        var msg = $"#Polly #FallbackAsync Fallback method used due to: {ex}";
                        Log.Error(msg, ex);
                        return Task.CompletedTask;
                    })
                .WithPolicyKey(PolicyKeys.SqlFallbackAsyncPolicy);

        /// <summary>
        /// Handles a fallback policy for SqlException, TimeoutRejectedException and BrokenCircuitException exceptions. It means, if the process fails for one of those reasons, the fallback method is executed instead.
        /// </summary>
        public static IAsyncPolicy GetFallbackPolicy(Func<Task> action) =>
            Policy
                .Handle<SqlException>(ex => SqlTransientErrors.Contains(ex.Number))
                .Or<SqlException>(ex => SqlTransactionErrors.Contains(ex.Number))
                .Or<TimeoutRejectedException>()
                .Or<BrokenCircuitException>()
                .FallbackAsync(cancellationToken => action(),
                    ex =>
                    {
                        var msg = $"#Polly #FallbackAsync Fallback method used due to: {ex}";
                        Log.Error(msg, ex);
                        return Task.CompletedTask;
                    })
                .WithPolicyKey(PolicyKeys.SqlFallbackAsyncPolicy);

        private static void OnHalfOpen()
        {
            Log.Warn("#Polly #CircuitBreakerAsync Half-open: Next call is a trial");
        }

        private static void OnReset()
        {
            // on circuit closed
            Log.Warn("#Polly #CircuitBreakerAsync Circuit breaker reset");
        }

        private static void OnBreak(Exception exception, TimeSpan duration)
        {
            // on circuit opened
            Log.Warn("#Polly #CircuitBreakerAsync Circuit breaker opened", exception);
        }
    }
}
