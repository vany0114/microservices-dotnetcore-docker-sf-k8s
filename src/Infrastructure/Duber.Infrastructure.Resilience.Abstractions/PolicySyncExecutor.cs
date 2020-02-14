using System;
using System.Collections.Generic;
using System.Linq;
using Polly;
using Polly.Registry;

namespace Duber.Infrastructure.Resilience.Abstractions
{
    /// <summary>
    /// Executes the action applying all the policies defined in the wrapper
    /// </summary>
    public class PolicySyncExecutor : IPolicySyncExecutor
    {
        private readonly IEnumerable<ISyncPolicy> _syncPolicies;

        public PolicyRegistry PolicyRegistry { get; set; }

        public PolicySyncExecutor(IEnumerable<ISyncPolicy> policies)
        {
            _syncPolicies = policies ?? throw new ArgumentNullException(nameof(policies));

            PolicyRegistry = new PolicyRegistry
            {
                [nameof(PolicySyncExecutor)] = Policy.Wrap(_syncPolicies.ToArray())
            };
        }

        public T Execute<T>(Func<T> action)
        {
            var policy = PolicyRegistry.Get<ISyncPolicy>(nameof(PolicySyncExecutor));
            return policy.Execute(action);
        }

        public void Execute(Action action)
        {
            var policy = PolicyRegistry.Get<ISyncPolicy>(nameof(PolicySyncExecutor));
            policy.Execute(action);
        }

        public T Execute<T>(Func<Context, T> action, Context context)
        {
            var policy = PolicyRegistry.Get<ISyncPolicy>(nameof(PolicySyncExecutor));
            return policy.Execute(action, context);
        }

        public void Execute(Action<Context> action, Context context)
        {
            var policy = PolicyRegistry.Get<ISyncPolicy>(nameof(PolicySyncExecutor));
            policy.Execute(action, context);
        }
    }
}
