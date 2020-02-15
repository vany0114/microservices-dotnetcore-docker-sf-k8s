using System;
using System.Collections.Generic;
using System.Linq;
using Duber.Infrastructure.Resilience.Abstractions;
using Duber.Infrastructure.Resilience.Sql.Policies;
using Polly;
using Polly.Registry;

namespace Duber.Infrastructure.Resilience.Sql
{
    /// <summary>
    /// The order which is inserted the policies into the list matters.
    /// https://github.com/App-vNext/Polly/wiki/PolicyWrap#usage-recommendations
    /// </summary>
    internal class SqlSyncPolicyBuilder : ISqlSyncPolicyBuilder
    {
        private List<ISyncPolicy> _policies;
        private const int RetryCount = 5;
        private readonly bool _sharedPolicies;
        private const int ExceptionsAllowedBeforeBreaking = 3;
        private readonly TimeSpan _overallTimeout = GetTimeout();
        private static readonly PolicyRegistry _registry = new PolicyRegistry();

        public SqlSyncPolicyBuilder(bool sharePolicies = false)
        {
            _sharedPolicies = sharePolicies;
            _policies = new List<ISyncPolicy>();
        }

        public ISqlSyncPolicyBuilder WithDefaultPolicies()
        {
            _policies.Add(SyncPolicies.GetTimeOutPolicy(_overallTimeout, PolicyKeys.SqlOverallTimeoutSyncPolicy));
            _policies.Add(SyncPolicies.GetCommonTransientErrorsPolicies(RetryCount));
            _policies.AddRange(SyncPolicies.GetCircuitBreakerPolicies(ExceptionsAllowedBeforeBreaking));
            return this;
        }

        public ISqlSyncPolicyBuilder WithTransientErrors(int retryCount)
        {
            _policies.Add(SyncPolicies.GetCommonTransientErrorsPolicies(RetryCount));
            return this;
        }

        public ISqlSyncPolicyBuilder WithTransaction()
        {
            _policies.Add(SyncPolicies.GetTransactionPolicy(RetryCount));
            return this;
        }

        public ISqlSyncPolicyBuilder WithCircuitBreaker(int exceptionsAllowedBeforeBreaking)
        {
            _policies.AddRange(SyncPolicies.GetCircuitBreakerPolicies(ExceptionsAllowedBeforeBreaking));
            return this;
        }

        public ISqlSyncPolicyBuilder WithFallback<T>(Func<T> action)
        {
            _policies.Add(SyncPolicies.GetFallbackPolicy(action));
            return this;
        }

        public ISqlSyncPolicyBuilder WithFallback(Action action)
        {
            _policies.Add(SyncPolicies.GetFallbackPolicy(action));
            return this;
        }

        public ISqlSyncPolicyBuilder WithOverallTimeout(TimeSpan timeout)
        {
            _policies.Add(SyncPolicies.GetTimeOutPolicy(timeout, PolicyKeys.SqlOverallTimeoutSyncPolicy));
            return this;
        }

        public ISqlSyncPolicyBuilder WithTimeoutPerRetry(TimeSpan timeout)
        {
            _policies.Add(SyncPolicies.GetTimeOutPolicy(timeout, PolicyKeys.SqlTimeoutPerRetrySyncPolicy));
            return this;
        }

        public ISqlSyncPolicyBuilder WithOverallAndTimeoutPerRetry(TimeSpan overallTimeout, TimeSpan timeoutPerRetry)
        {
            _policies.Add(SyncPolicies.GetTimeOutPolicy(_overallTimeout, PolicyKeys.SqlOverallTimeoutSyncPolicy));
            _policies.Add(SyncPolicies.GetTimeOutPolicy(timeoutPerRetry, PolicyKeys.SqlTimeoutPerRetrySyncPolicy));
            return this;
        }

        public IPolicySyncExecutor Build()
        {
            if (!_policies.Any())
                throw new InvalidOperationException("There are no policies to execute.");

            // to prevent consumers uses WithDefaultPolicies together with other methods.
            var duplicatedPolicies = _policies.GroupBy(x => x.PolicyKey)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key)
                .ToList();

            if (duplicatedPolicies.Any())
                throw new InvalidOperationException("There are duplicated policies. When you use WithDefaultPolicies method, you can't use either WithTransientErrors. WithCircuitBreaker or WithOverallTimeout methods at the same time, because those policies are already included.");

            // if there is timeout per retry but there's not retry policy.
            var retryPolicyNames = new[] { PolicyKeys.SqlCommonTransientErrorsSyncPolicy, PolicyKeys.SqlTransactionSyncPolicy };
            var retryPolicies = _policies.Where(x => retryPolicyNames.Contains(x.PolicyKey));
            var timeoutPerRetryPolicy = _policies.Where(x => x.PolicyKey == PolicyKeys.SqlTimeoutPerRetrySyncPolicy);

            if (timeoutPerRetryPolicy.Any() && !retryPolicies.Any())
                throw new InvalidOperationException("You're trying to use Timeout per retries but you don't have Retry policies configured.");

            // The order of policies into the list is important (not mandatory) in order to get a consistent resilience strategy.
            // That's why I named the policies alphabetically.
            // https://github.com/App-vNext/Polly/wiki/PolicyWrap#usage-recommendations
            _policies = _policies.OrderBy(x => x.PolicyKey).ToList();

            if (_sharedPolicies)
            {
                for (var index = 0; index < _policies.Count; index++)
                {
                    var policy = _policies[index];
                    _registry.TryGet(policy.PolicyKey, out ISyncPolicy sharedPolicy);

                    if (sharedPolicy == null)
                    {
                        _registry.Add(policy.PolicyKey, policy);
                    }
                    else
                    {
                        // replaces new policy to the one already exists into register
                        _policies[index] = sharedPolicy;
                    }
                }
            }

            return new PolicySyncExecutor(_policies);
        }

        /// <summary>
        /// Gets the timeout based on retries and exponential back-off
        /// </summary>
        private static TimeSpan GetTimeout()
        {
            var retry = 1;
            var delay = TimeSpan.Zero;
            while (retry <= RetryCount)
            {
                delay += TimeSpan.FromSeconds(Math.Pow(2, retry));
                retry++;
            }

            // plus an arbitrary max time the operation could take
            return delay + TimeSpan.FromSeconds(10);
        }
    }
}
