using System;
using System.Data.SqlClient;
using System.Linq;
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
    internal class SyncPolicies
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SyncPolicies));
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
        public static ISyncPolicy GetCommonTransientErrorsPolicies(int retryCount) =>
            Policy
                .Handle<SqlException>(ex => SqlTransientErrors.Contains(ex.Number))
                .WaitAndRetry(
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
                        var msg = $"#Polly #WaitAndRetry Retry {retries}" +
                                  $"of {context.PolicyKey} " +
                                  $"due to: {exception}.";
                        Log.Error(msg, exception);
                    })
                .WithPolicyKey(PolicyKeys.SqlCommonTransientErrorsSyncPolicy);

        /// <summary>
        /// Gets a Retry policy for the most common transaction errors in Azure Sql.
        /// </summary>
        public static ISyncPolicy GetTransactionPolicy(int retryCount) =>
            Policy
                .Handle<SqlException>(ex => SqlTransactionErrors.Contains(ex.Number))
                .WaitAndRetry(
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
                        var msg = $"#Polly #WaitAndRetrySync Retry {retries}" +
                                  $"of {context.PolicyKey} " +
                                  $"due to: {exception}.";
                        Log.Error(msg, exception);
                    })
                .WithPolicyKey(PolicyKeys.SqlTransactionSyncPolicy);

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
        public static ISyncPolicy[] GetCircuitBreakerPolicies(int exceptionsAllowedBeforeBreaking)
            => new ISyncPolicy[]
            {
                Policy
                    .Handle<SqlException>(ex => ex.Number == (int)SqlHandledExceptions.DatabaseNotCurrentlyAvailable)
                    .CircuitBreaker(
                        // number of exceptions before breaking circuit
                        exceptionsAllowedBeforeBreaking,
                        // time circuit opened before retry
                        TimeSpan.FromSeconds(30),
                        OnBreak,
                        OnReset,
                        OnHalfOpen)
                    .WithPolicyKey($"F1.{PolicyKeys.SqlCircuitBreakerSyncPolicy}"),
                Policy
                    .Handle<SqlException>(ex => ex.Number == (int)SqlHandledExceptions.ErrorProcessingRequest)
                    .CircuitBreaker(
                        // number of exceptions before breaking circuit
                        exceptionsAllowedBeforeBreaking,
                        // time circuit opened before retry
                        TimeSpan.FromSeconds(30),
                        OnBreak,
                        OnReset,
                        OnHalfOpen)
                    .WithPolicyKey($"F2.{PolicyKeys.SqlCircuitBreakerSyncPolicy}"),
                Policy
                    .Handle<SqlException>(ex => ex.Number == (int)SqlHandledExceptions.ServiceCurrentlyBusy)
                    .CircuitBreaker(
                        // number of exceptions before breaking circuit
                        exceptionsAllowedBeforeBreaking,
                        // time circuit opened before retry
                        TimeSpan.FromSeconds(30),
                        OnBreak,
                        OnReset,
                        OnHalfOpen)
                    .WithPolicyKey($"F3.{PolicyKeys.SqlCircuitBreakerSyncPolicy}"),
                Policy
                    .Handle<SqlException>(ex => ex.Number == (int)SqlHandledExceptions.NotEnoughResources)
                    .CircuitBreaker(
                        // number of exceptions before breaking circuit
                        exceptionsAllowedBeforeBreaking,
                        // time circuit opened before retry
                        TimeSpan.FromSeconds(30),
                        OnBreak,
                        OnReset,
                        OnHalfOpen)
                    .WithPolicyKey($"F4.{PolicyKeys.SqlCircuitBreakerSyncPolicy}"),
                Policy
                    .Handle<SqlException>(ex => ex.Number == (int)SqlHandledExceptions.SessionTerminatedLongTransaction)
                    .CircuitBreaker(
                        // number of exceptions before breaking circuit
                        exceptionsAllowedBeforeBreaking,
                        // time circuit opened before retry
                        TimeSpan.FromSeconds(30),
                        OnBreak,
                        OnReset,
                        OnHalfOpen)
                    .WithPolicyKey($"F5.{PolicyKeys.SqlCircuitBreakerSyncPolicy}"),
                Policy
                    .Handle<SqlException>(ex => ex.Number == (int)SqlHandledExceptions.SessionTerminatedToManyLocks)
                    .CircuitBreaker(
                        // number of exceptions before breaking circuit
                        exceptionsAllowedBeforeBreaking,
                        // time circuit opened before retry
                        TimeSpan.FromSeconds(30),
                        OnBreak,
                        OnReset,
                        OnHalfOpen)
                    .WithPolicyKey($"F6.{PolicyKeys.SqlCircuitBreakerSyncPolicy}")
            };

        /// <summary>
        /// Gets a pessimistic timeout policy in order to cancel the process which doesn't even honor cancellation.
        /// https://github.com/App-vNext/Polly/wiki/Timeout#pessimistic-timeout
        /// </summary>
        public static ISyncPolicy GetTimeOutPolicy(TimeSpan timeout, string policyName) =>
            Policy
                .Timeout(
                    timeout,
                    TimeoutStrategy.Pessimistic)
                .WithPolicyKey(policyName);

        /// <summary>
        /// Handles a fallback policy for SqlException, TimeoutRejectedException and BrokenCircuitException exceptions. It means, if the process fails for one of those reasons, the fallback method is executed instead.
        /// </summary>
        public static ISyncPolicy GetFallbackPolicy<T>(Func<T> action) =>
            Policy
                .Handle<SqlException>(ex => SqlTransientErrors.Contains(ex.Number))
                .Or<SqlException>(ex => SqlTransactionErrors.Contains(ex.Number))
                .Or<TimeoutRejectedException>()
                .Or<BrokenCircuitException>()
                .Fallback(() => action(),
                    ex =>
                    {
                        var msg = $"#Polly #Fallback Fallback method used due to: {ex}";
                        Log.Error(msg, ex);
                    })
                .WithPolicyKey(PolicyKeys.SqlFallbackSyncPolicy);

        /// <summary>
        /// Handles a fallback policy for SqlException, TimeoutRejectedException and BrokenCircuitException exceptions. It means, if the process fails for one of those reasons, the fallback method is executed instead.
        /// </summary>
        public static ISyncPolicy GetFallbackPolicy(Action action) =>
            Policy
                .Handle<SqlException>(ex => SqlTransientErrors.Contains(ex.Number))
                .Or<SqlException>(ex => SqlTransactionErrors.Contains(ex.Number))
                .Or<TimeoutRejectedException>()
                .Or<BrokenCircuitException>()
                .Fallback(action,
                    ex =>
                    {
                        var msg = $"#Polly #Fallback Fallback method used due to: {ex}";
                        Log.Error(msg, ex);
                    })
                .WithPolicyKey(PolicyKeys.SqlFallbackSyncPolicy);

        private static void OnHalfOpen()
        {
            Log.Warn("#Polly #CircuitBreakerSync Half-open: Next call is a trial");
        }

        private static void OnReset()
        {
            // on circuit closed
            Log.Warn("#Polly #CircuitBreakerSync Circuit breaker reset");
        }

        private static void OnBreak(Exception exception, TimeSpan duration)
        {
            // on circuit opened
            Log.Warn("#Polly #CircuitBreakerSync Circuit breaker opened", exception);
        }
    }
}
