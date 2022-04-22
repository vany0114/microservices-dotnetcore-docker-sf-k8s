using System;

namespace Duber.Invoice.API.Application.Model
{
    public class CreateInvoiceRequest
    {
        public PaymentMethod PaymentMethod { get; set; }

        public Guid TripId { get; set; }

        public TimeSpan Duration { get; set; }

        public double Distance { get; set; }

        public TripStatus TripStatus { get; set; }

        public int UserId { get; set; }
    }
}
