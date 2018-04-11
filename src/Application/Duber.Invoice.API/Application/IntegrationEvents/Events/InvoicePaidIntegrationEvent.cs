using System;
using Duber.Infrastructure.EventBus.Events;
using Duber.Invoice.API.Application.Model;

namespace Duber.Invoice.API.Application.IntegrationEvents.Events
{
    public class InvoicePaidIntegrationEvent : IntegrationEvent
    {
        public InvoicePaidIntegrationEvent(Guid invoiceId, PaymentStatus status, string cardNumber, string cardType)
        {
            InvoiceId = invoiceId;
            Status = status;
            CardNumber = cardNumber;
            CardType = cardType;
        }

        public Guid InvoiceId { get; }

        public PaymentStatus Status { get; }

        public string CardNumber { get; }

        public string CardType { get; }
    }
}
