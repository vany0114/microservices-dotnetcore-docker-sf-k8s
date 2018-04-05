using System;
using Duber.Infrastructure.EventBus.Events;
using Duber.Trip.API.Application.Model;

namespace Duber.Trip.API.Application.IntegrationEvents
{
    public class TripCancelledIntegrationEvent : IntegrationEvent
    {
        public TripCancelledIntegrationEvent(Guid tripId, DateTime tripCreationTime, PaymentMethod paymentMethod)
        {
            TripCreationTime = tripCreationTime;
            PaymentMethod = paymentMethod;
            TripId = tripId;
        }

        public Guid TripId { get; }

        public DateTime TripCreationTime { get; }

        public PaymentMethod PaymentMethod { get; }
    }
}
