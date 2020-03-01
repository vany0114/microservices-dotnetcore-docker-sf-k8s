using System;
using System.Threading.Tasks;
using Duber.Domain.Invoice.Repository;
using Duber.Domain.Invoice.Services;
using Duber.Domain.SharedKernel.Model;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Invoice.API.Application.IntegrationEvents.Events;
using Microsoft.Extensions.Logging;

namespace Duber.Invoice.API.Application.IntegrationEvents.Hnadlers
{
    public class TripFinishedIntegrationEventHandler : IIntegrationEventHandler<TripFinishedIntegrationEvent>
    {
        private readonly IPaymentService _paymentService;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ILogger<TripFinishedIntegrationEventHandler> _logger;

        public TripFinishedIntegrationEventHandler(IInvoiceRepository invoiceRepository, IPaymentService paymentService, ILogger<TripFinishedIntegrationEventHandler> logger)
        {
            _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _logger = logger;
        }

        public async Task Handle(TripFinishedIntegrationEvent @event)
        {
            _logger.LogInformation($"Trip {@event.TripId} has finished.");

            var invoice = await _invoiceRepository.GetInvoiceByTripAsync(@event.TripId);
            if (invoice != null) return;

            try
            {
                invoice = new Domain.Invoice.Model.Invoice(
                    @event.PaymentMethod.Id,
                    @event.TripId,
                    @event.Duration,
                    @event.Distance,
                    TripStatus.Finished.Id);

                await _invoiceRepository.AddInvoiceAsync(invoice);
                _logger.LogInformation($"Invoice {invoice.InvoiceId} created.");

                // integration with external payment system.
                if (Equals(invoice.PaymentMethod, PaymentMethod.CreditCard) && invoice.Total > 0)
                {
                    await _paymentService.PerformPayment(invoice, @event.UserId);
                    _logger.LogInformation($"Payment for invoice {invoice.InvoiceId} has been processed.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error trying to perform the payment. Trip: {@event.TripId}", ex);
            }
            finally
            {
                _invoiceRepository.Dispose();
            }
        }
    }
}
