using Duber.Infrastructure.Resilience.Abstractions;
using Polly.Registry;
using System;
using System.Threading.Tasks;

namespace Duber.Infrastructure.Resilience.Sql
{
    public interface ISqlAsyncPolicyBuilder
    {
        ISqlAsyncPolicyBuilder WithDefaultPolicies();

        ISqlAsyncPolicyBuilder WithFallback<T>(Func<Task<T>> action);

        ISqlAsyncPolicyBuilder WithFallback(Func<Task> action);

        ISqlAsyncPolicyBuilder WithOverallTimeout(TimeSpan timeout);

        ISqlAsyncPolicyBuilder WithTimeoutPerRetry(TimeSpan timeout);

        ISqlAsyncPolicyBuilder WithOverallAndTimeoutPerRetry(TimeSpan overallTimeout, TimeSpan timeoutPerRetry);

        ISqlAsyncPolicyBuilder WithTransientErrors(int retryCount = 4);

        ISqlAsyncPolicyBuilder WithTransaction();

        ISqlAsyncPolicyBuilder WithCircuitBreaker(int exceptionsAllowedBeforeBreaking = 3);

        IPolicyAsyncExecutor Build();
    }

    public interface ISqlSyncPolicyBuilder
    {
        ISqlSyncPolicyBuilder WithDefaultPolicies();

        ISqlSyncPolicyBuilder WithFallback<T>(Func<T> action);

        ISqlSyncPolicyBuilder WithFallback(Action action);

        ISqlSyncPolicyBuilder WithOverallTimeout(TimeSpan timeout);

        ISqlSyncPolicyBuilder WithTimeoutPerRetry(TimeSpan timeout);

        ISqlSyncPolicyBuilder WithOverallAndTimeoutPerRetry(TimeSpan overallTimeout, TimeSpan timeoutPerRetry);

        ISqlSyncPolicyBuilder WithTransientErrors(int retryCount = 4);

        ISqlSyncPolicyBuilder WithTransaction();

        ISqlSyncPolicyBuilder WithCircuitBreaker(int exceptionsAllowedBeforeBreaking = 3);

        IPolicySyncExecutor Build();
    }
}
