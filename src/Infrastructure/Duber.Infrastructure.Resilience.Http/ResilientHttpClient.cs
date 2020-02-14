using Polly;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Duber.Infrastructure.Resilience.Http
{
    public class ResilientHttpClient
    {
        private readonly HttpClient _client;

        public ResilientHttpClient(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, Context context)
        {
            // We attach the Polly context to the HttpRequestMessage using an extension method provided by HttpClientFactory.
            message.SetPolicyExecutionContext(context);

            // Make the request using the client configured by HttpClientFactory, which embeds the Polly and Simmy policies.
            return await _client.SendAsync(message);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage message)
        {
            // Make the request using the client configured by HttpClientFactory, which embeds the Polly and Simmy policies.
            return await _client.SendAsync(message);
        }
    }
}
