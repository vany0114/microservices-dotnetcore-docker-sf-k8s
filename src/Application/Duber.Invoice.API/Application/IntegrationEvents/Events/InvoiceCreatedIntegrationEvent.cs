using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duber.Infrastructure.EventBus.Events;

namespace Duber.Invoice.API.Application.IntegrationEvents.Events
{
    public class InvoiceCreatedIntegrationEvent : IntegrationEvent
    {
        public InvoiceCreatedIntegrationEvent(Guid invoiceId, decimal fee, decimal total)
        {
            InvoiceId = invoiceId;
            Fee = fee;
            Total = total;
        }

        public Guid InvoiceId { get; }

        public decimal Fee { get; }

        public decimal Total { get; }
    }
}
