using System;
using Duber.Infrastructure.EventBus.Events;

namespace Duber.Invoice.API.Application.IntegrationEvents.Events
{
    public class InvoiceCreatedIntegrationEvent : IntegrationEvent
    {
        public InvoiceCreatedIntegrationEvent(Guid invoiceId, decimal fee, decimal total, Guid tripId)
        {
            InvoiceId = invoiceId;
            Fee = fee;
            Total = total;
            TripId = tripId;
        }

        public Guid InvoiceId { get; }

        public Guid TripId { get; }

        public decimal Fee { get; }

        public decimal Total { get; }
    }
}
