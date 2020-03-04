using System;

namespace Duber.Trip.Notifications.Application.IntegrationEvents.Events
{
    public class TripFinishedIntegrationEvent : TripEventBase
    {
        public TripFinishedIntegrationEvent(Guid tripId, string connectionId) : base(tripId, connectionId)
        {
        }
    }
}
