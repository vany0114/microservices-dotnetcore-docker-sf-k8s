using System;
using Duber.Domain.SharedKernel.Model;
using MediatR;

namespace Duber.Domain.Invoice.Events
{
    public class InvoicePaidDomainEvent : INotification
    {
        public InvoicePaidDomainEvent(Guid invoiceId, PaymentStatus status, string cardNumber, string cardType, Guid tripId)
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
}
