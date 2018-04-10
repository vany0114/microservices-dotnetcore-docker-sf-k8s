using System;

namespace Duber.Invoice.API.Application.Model
{
    public class Invoice
    {
        public decimal Fee { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public decimal Total { get; set; }

        public Guid InvoiceId { get; set; }

        public TripInformation TripInformation { get; set; }

        public DateTime Created { get; set; }

        public PaymentInfo PaymentInfo { get; set; }
    }

    public class TripInformation
    {
        public TimeSpan Duration { get; set; }

        public double Distance { get; set; }

        public Guid Id { get; set; }

        public TripStatus Status { get; set; }
    }

    public class PaymentMethod
    {
        public string Name { get; set; }

        public int Id { get; set; }
    }

    public class TripStatus
    {
        public string Name { get; set; }

        public int Id { get; set; }
    }

    public class PaymentInfo
    {
        public int UserId { get; set; }

        public PaymentStatus Status { get; set; }

        public string CardNumber { get; set; }

        public string CardType { get; set; }
    }

    public enum PaymentStatus
    {
        Accepted = 1,
        Rejected = 2,
    }
}
