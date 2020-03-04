using System;

namespace Duber.Trip.Notifications.Application.IntegrationEvents.Events
{
    public class TripCreatedIntegrationEvent : TripEventBase
    {
        public TripCreatedIntegrationEvent(Guid tripId, string connectionId) : base(tripId, connectionId)
        {
        }
    }
}
