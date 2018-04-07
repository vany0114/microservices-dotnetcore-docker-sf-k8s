using System;
using Duber.Infrastructure.EventBus.Events;
using Duber.Invoice.API.Application.Model;

namespace Duber.Invoice.API.Application.IntegrationEvents.Events
{
    public class TripFinishedIntegrationEvent : IntegrationEvent
    {
        public TripFinishedIntegrationEvent(Guid tripId, double distance, TimeSpan duration, PaymentMethod paymentMethod)
        {
            Distance = distance;
            Duration = duration;
            PaymentMethod = paymentMethod;
            TripId = tripId;
        }

        public Guid TripId { get; }

        public double Distance { get; }

        public TimeSpan Duration { get; }

        public PaymentMethod PaymentMethod { get; }
    }
}
