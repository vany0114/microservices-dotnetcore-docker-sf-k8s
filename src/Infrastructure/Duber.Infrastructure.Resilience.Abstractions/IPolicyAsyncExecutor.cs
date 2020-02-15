using Polly;
using Polly.Registry;
using System;
using System.Threading.Tasks;

namespace Duber.Infrastructure.Resilience.Abstractions
{
    /// <summary>
    /// Polly doesn't support async void methods. So you can't pass an Action.
    /// http://www.thepollyproject.org/2017/06/09/polly-and-synchronous-versus-asynchronous-policies/
    /// Returns a Task in order to avoid execution of void methods asynchronously, which causes unexpected out-of-sequence execution of policy hooks and continuing policy actions, and a risk of unobserved exceptions.
    /// https://msdn.microsoft.com/en-us/magazine/jj991977.aspx
    /// https://github.com/App-vNext/Polly/issues/107#issuecomment-218835218
    /// </summary>
    public interface IPolicyAsyncExecutor
    {
        PolicyRegistry PolicyRegistry { get; set; }

        Task<T> ExecuteAsync<T>(Func<Task<T>> action);

        Task<T> ExecuteAsync<T>(Func<Context, Task<T>> action, Context context);

        Task ExecuteAsync(Func<Task> action);

        Task ExecuteAsync(Func<Context, Task> action, Context context);
    }
}
