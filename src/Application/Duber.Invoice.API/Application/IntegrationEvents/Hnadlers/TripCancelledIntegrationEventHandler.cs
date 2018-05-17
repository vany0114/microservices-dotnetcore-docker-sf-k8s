using System;
using System.Threading.Tasks;
using Duber.Domain.Invoice.Repository;
using Duber.Domain.Invoice.Services;
using Duber.Domain.SharedKernel.Model;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Invoice.API.Application.IntegrationEvents.Events;

namespace Duber.Invoice.API.Application.IntegrationEvents.Hnadlers
{
    public class TripCancelledIntegrationEventHandler : IIntegrationEventHandler<TripCancelledIntegrationEvent>
    {
        private readonly IPaymentService _paymentService;
        private readonly IInvoiceRepository _invoiceRepository;

        public TripCancelledIntegrationEventHandler(IInvoiceRepository invoiceRepository, IPaymentService paymentService)
        {
            _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        }

        public async Task Handle(TripCancelledIntegrationEvent @event)
        {
            var invoice = await _invoiceRepository.GetInvoiceByTripAsync(@event.TripId);
            if (invoice != null) return;

            try
            {
                invoice = new Domain.Invoice.Model.Invoice(
                    @event.PaymentMethod.Id,
                    @event.TripId,
                    @event.Duration,
                    0,
                    TripStatus.Cancelled.Id);

                await _invoiceRepository.AddInvoiceAsync(invoice);

                // integration with external payment system.
                if (Equals(invoice.PaymentMethod, PaymentMethod.CreditCard) && invoice.Total > 0)
                {
                    await _paymentService.PerformPayment(invoice, @event.UserId);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error trying to perform tge payment the Trip: {@event.TripId}", ex);
            }
            finally
            {
                _invoiceRepository.Dispose();
            }
        }
    }
}
