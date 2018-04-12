using System;
using Duber.Infrastructure.EventBus.Events;

namespace Duber.WebSite.Application.IntegrationEvents.Events
{
    public class InvoicePaidIntegrationEvent : IntegrationEvent
    {
        public InvoicePaidIntegrationEvent(Guid invoiceId, PaymentStatus status, string cardNumber, string cardType, Guid tripId)
        {
            InvoiceId = invoiceId;
            Status = status;
            CardNumber = cardNumber;
            CardType = cardType;
            TripId = tripId;
        }

        public Guid InvoiceId { get; }

        public Guid TripId { get; }

        public PaymentStatus Status { get; }

        public string CardNumber { get; }

        public string CardType { get; }
    }

    public enum PaymentStatus
    {
        Accepted = 1,
        Rejected = 2,
    }
}
