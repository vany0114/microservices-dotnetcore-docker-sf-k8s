using Duber.Infrastructure.EventBus.Events;
using System;
using MediatR;

namespace Duber.Domain.Invoice.Events
{
    public class InvoiceCreatedDomainEvent : INotification
    {
        public InvoiceCreatedDomainEvent(Guid invoiceId, decimal fee, decimal total)
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
