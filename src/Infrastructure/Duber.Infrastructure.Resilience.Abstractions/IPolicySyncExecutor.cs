using Polly;
using Polly.Registry;
using System;

namespace Duber.Infrastructure.Resilience.Abstractions
{
    public interface IPolicySyncExecutor
    {
        PolicyRegistry PolicyRegistry { get; set; }

        T Execute<T>(Func<T> action);

        T Execute<T>(Func<Context, T> action, Context context);

        void Execute(Action action);

        void Execute(Action<Context> action, Context context);
    }
}
