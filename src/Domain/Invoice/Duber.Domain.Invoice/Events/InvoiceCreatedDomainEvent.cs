using Duber.Infrastructure.EventBus.Events;
using System;
using MediatR;

namespace Duber.Domain.Invoice.Events
{
    public class InvoiceCreatedDomainEvent : INotification
    {
        public InvoiceCreatedDomainEvent(Guid invoiceId, decimal fee, decimal total, bool paidWithCreditCard)
        {
            InvoiceId = invoiceId;
            Fee = fee;
            Total = total;
            PaidWithCreditCard = paidWithCreditCard;
        }

        public Guid InvoiceId { get; }

        public decimal Fee { get; }

        public decimal Total { get; }

        public bool PaidWithCreditCard { get; }
    }
}
