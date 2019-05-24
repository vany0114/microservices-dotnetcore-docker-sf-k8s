using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using RestSharp;

namespace Duber.Infrastructure.Resilience.Http
{
    public class ResilientHttpInvoker
    {
        private readonly IEnumerable<IAsyncPolicy> _policies;

        public ResilientHttpInvoker(IEnumerable<IAsyncPolicy> policies)
        {
            _policies = policies ?? throw new ArgumentNullException(nameof(policies));
        }

        public Task<IRestResponse> InvokeAsync(Func<Task<IRestResponse>> action)
        {
            return HttpInvoker(async () =>
            {
                var response = await action.Invoke();

                // raise exception if HttpResponseCode 500 
                // needed for circuit breaker to track fails
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new HttpRequestException();
                }

                return response;
            });
        }

        private async Task<T> HttpInvoker<T>(Func<Task<T>> action)
        {
            // Executes the action applying all the policies defined in the wrapper
            var policyWrap = Policy.WrapAsync(_policies.ToArray());
            return await policyWrap.ExecuteAsync(async () => await action());
        }
    }
}
