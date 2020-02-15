using System;
using System.Net.Http;
using System.Threading.Tasks;
using Duber.Domain.ACL.Contracts;
using Duber.Domain.ACL.Translators;
using Duber.Domain.SharedKernel.Model;
using Duber.Infrastructure.Resilience.Http;

namespace Duber.Domain.ACL.Adapters
{
    public class PaymentServiceAdapter : IPaymentServiceAdapter
    {
        private readonly ResilientHttpClient _httpClient;
        private readonly string _paymentServiceBaseUrl;

        public PaymentServiceAdapter(ResilientHttpClient httpClient, string paymentServiceBaseUrl)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _paymentServiceBaseUrl = !string.IsNullOrWhiteSpace(paymentServiceBaseUrl) ? paymentServiceBaseUrl : throw new ArgumentNullException(nameof(paymentServiceBaseUrl));
        }

        public async Task<PaymentInfo> ProcessPaymentAsync(int userId, string reference)
        {
            var uri = new Uri(
                new Uri(_paymentServiceBaseUrl),
                string.Format(ThirdPartyServices.Payment.PerformPayment(), userId, reference));

            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
            return PaymentInfoTranslator.Translate(await response.Content.ReadAsStringAsync());
        }
    }
}