using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using Polly.Registry;

namespace Duber.Infrastructure.Resilience.Abstractions
{
    /// <summary>
    /// Executes the action applying all the policies defined in the wrapper
    /// </summary>
    public class PolicyAsyncExecutor : IPolicyAsyncExecutor
    {
        private readonly IEnumerable<IAsyncPolicy> _asyncPolicies;

        public PolicyRegistry PolicyRegistry { get; set; }

        public PolicyAsyncExecutor(IEnumerable<IAsyncPolicy> policies)
        {
            _asyncPolicies = policies ?? throw new ArgumentNullException(nameof(policies));

            PolicyRegistry = new PolicyRegistry
            {
                [nameof(PolicyAsyncExecutor)] = Policy.WrapAsync(_asyncPolicies.ToArray())
            };
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            var policy = PolicyRegistry.Get<IAsyncPolicy>(nameof(PolicyAsyncExecutor));
            return await policy.ExecuteAsync(action);
        }

        public async Task ExecuteAsync(Func<Task> action)
        {
            var policy = PolicyRegistry.Get<IAsyncPolicy>(nameof(PolicyAsyncExecutor));
            await policy.ExecuteAsync(action);
        }

        public async Task<T> ExecuteAsync<T>(Func<Context, Task<T>> action, Context context)
        {
            var policy = PolicyRegistry.Get<IAsyncPolicy>(nameof(PolicyAsyncExecutor));
            return await policy.ExecuteAsync(action, context);
        }

        public async Task ExecuteAsync(Func<Context, Task> action, Context context)
        {
            var policy = PolicyRegistry.Get<IAsyncPolicy>(nameof(PolicyAsyncExecutor));
            await policy.ExecuteAsync(action, context);
        }
    }
}
