using System;
using System.Net;
using System.Threading.Tasks;
using Duber.Domain.ACL.Contracts;
using Duber.Domain.ACL.Translators;
using Duber.Domain.SharedKernel.Model;
using Duber.Infrastructure.Resilience.Http;
using RestSharp;

namespace Duber.Domain.ACL.Adapters
{
    public class PaymentServiceAdapter : IPaymentServiceAdapter
    {
        private readonly ResilientHttpInvoker _httpInvoker;
        private readonly string _paymentServiceBaseUrl;

        public PaymentServiceAdapter(ResilientHttpInvoker httpInvoker, string paymentServiceBaseUrl)
        {
            _httpInvoker = httpInvoker ?? throw new ArgumentNullException(nameof(httpInvoker));
            _paymentServiceBaseUrl = !string.IsNullOrWhiteSpace(paymentServiceBaseUrl) ? paymentServiceBaseUrl : throw new ArgumentNullException(nameof(paymentServiceBaseUrl));
        }

        public async Task<PaymentInfo> ProcessPaymentAsync(int userId, string reference)
        {
            var response = await _httpInvoker.InvokeAsync(async () =>
            {
                var client = new RestClient(_paymentServiceBaseUrl);
                var request = new RestRequest(ThirdPartyServices.Payment.PerformPayment(), Method.POST);
                request.AddUrlSegment(nameof(userId), userId);
                request.AddUrlSegment(nameof(reference), reference);

                return await client.ExecuteTaskAsync(request);
            });

            if (response.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException("There was an error trying to perform the payment.", response.ErrorException);

            return PaymentInfoTranslator.Translate(response.Content);
        }
    }
}