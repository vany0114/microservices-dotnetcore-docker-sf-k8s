using System;
using Duber.Infrastructure.EventBus.Events;
using Duber.Trip.API.Application.Model;

namespace Duber.Trip.API.Application.IntegrationEvents
{
    public class TripFinishedIntegrationEvent : IntegrationEvent
    {
        public TripFinishedIntegrationEvent(Guid tripId, double distance, TimeSpan duration, PaymentMethod paymentMethod, int userId)
        {
            Distance = distance;
            Duration = duration;
            PaymentMethod = paymentMethod;
            UserId = userId;
            TripId = tripId;
        }

        public Guid TripId { get; }

        public double Distance { get; }

        public TimeSpan Duration { get; }

        public PaymentMethod PaymentMethod { get; }

        public int UserId { get; }
    }
}
