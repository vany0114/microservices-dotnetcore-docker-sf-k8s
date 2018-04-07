using System;
using Duber.Infrastructure.EventBus.Events;
using Duber.Invoice.API.Application.Model;

namespace Duber.Invoice.API.Application.IntegrationEvents.Events
{
    public class TripCancelledIntegrationEvent : IntegrationEvent
    {
        public TripCancelledIntegrationEvent(Guid tripId, TimeSpan duration, PaymentMethod paymentMethod)
        {
            Duration = duration;
            PaymentMethod = paymentMethod;
            TripId = tripId;
        }

        public Guid TripId { get; }

        public TimeSpan Duration { get; }

        public PaymentMethod PaymentMethod { get; }
    }
}
