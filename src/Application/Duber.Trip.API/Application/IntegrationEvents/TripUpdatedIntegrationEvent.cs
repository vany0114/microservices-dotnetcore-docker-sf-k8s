using System;
using Duber.Infrastructure.EventBus.Events;
using Duber.Trip.API.Application.Model;

namespace Duber.Trip.API.Application.IntegrationEvents
{
    public class TripUpdatedIntegrationEvent : IntegrationEvent
    {
        public TripUpdatedIntegrationEvent(Action action, TripStatus status, DateTime? started, DateTime? ended, Location currentLocation)
        {
            Action = action;
            Status = status;
            Started = started;
            Ended = ended;
            CurrentLocation = currentLocation;
        }

        public Action Action { get; }

        public TripStatus Status { get; }

        public DateTime? Started { get; }

        public DateTime? Ended { get; }

        public Location CurrentLocation { get; }
    }

    public enum Action
    {
        Accepted = 1,
        Started = 2,
        Cancelled = 3,
        FinishedEarlier = 4,
        UpdatedCurrentLocation = 5
    }
}
