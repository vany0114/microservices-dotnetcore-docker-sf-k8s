using System;
using Duber.Infrastructure.EventBus.Events;
using Duber.Invoice.API.Application.Model;

namespace Duber.Invoice.API.Application.IntegrationEvents.Events
{
    public class TripCancelledIntegrationEvent : IntegrationEvent
    {
        public TripCancelledIntegrationEvent(Guid tripId, TimeSpan duration, PaymentMethod paymentMethod, int userId)
        {
            Duration = duration;
            PaymentMethod = paymentMethod;
            UserId = userId;
            TripId = tripId;
        }

        public Guid TripId { get; }

        public TimeSpan Duration { get; }

        public PaymentMethod PaymentMethod { get; }

        public int UserId { get; }
    }
}
