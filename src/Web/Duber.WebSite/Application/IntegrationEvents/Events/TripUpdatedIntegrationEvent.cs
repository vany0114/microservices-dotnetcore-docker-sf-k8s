using System;
using Duber.Infrastructure.EventBus.Events;

namespace Duber.WebSite.Application.IntegrationEvents.Events
{
    public class TripUpdatedIntegrationEvent : IntegrationEvent
    {
        public TripUpdatedIntegrationEvent(Guid tripId, Action action, TripStatus status, DateTime? started, DateTime? ended, Location currentLocation, double? distance, TimeSpan? duration)
        {
            Action = action;
            Status = status;
            Started = started;
            Ended = ended;
            CurrentLocation = currentLocation;
            Distance = distance;
            Duration = duration;
            TripId = tripId;
        }

        public Guid TripId { get; }

        public Action Action { get; }

        public TripStatus Status { get; }

        public DateTime? Started { get; }

        public DateTime? Ended { get; }

        public Location CurrentLocation { get; }

        public double? Distance { get; }

        public TimeSpan? Duration { get; }
    }

    public enum Action
    {
        Accepted = 1,
        Started = 2,
        Cancelled = 3,
        FinishedEarlier = 4,
        UpdatedCurrentLocation = 5
    }

    public class TripStatus
    {
        public string Name { get; set; }

        public int Id { get; set; }
    }
}
