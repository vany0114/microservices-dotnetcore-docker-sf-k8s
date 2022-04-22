using System;
using Duber.Infrastructure.EventBus.Events;

namespace Duber.Trip.Notifications.Application.IntegrationEvents.Events
{
    public class TripEventBase : IntegrationEvent
    {
        public TripEventBase(Guid tripId, string connectionId)
        {
            TripId = tripId;
            ConnectionId = connectionId;
        }

        public string ConnectionId { get; }

        public Guid TripId { get; }
    }
}
