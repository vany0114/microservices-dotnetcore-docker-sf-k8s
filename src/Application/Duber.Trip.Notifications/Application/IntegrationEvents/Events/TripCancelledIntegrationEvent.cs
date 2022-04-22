using System;

namespace Duber.Trip.Notifications.Application.IntegrationEvents.Events
{
    public class TripCancelledIntegrationEvent : TripEventBase
    {
        public TripCancelledIntegrationEvent(Guid tripId, string connectionId) : base(tripId, connectionId)
        {
        }
    }
}
