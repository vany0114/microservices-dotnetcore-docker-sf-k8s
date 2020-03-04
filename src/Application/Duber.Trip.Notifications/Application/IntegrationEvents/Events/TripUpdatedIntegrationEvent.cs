using System;

namespace Duber.Trip.Notifications.Application.IntegrationEvents.Events
{
    public class TripUpdatedIntegrationEvent : TripEventBase
    {
        public TripUpdatedIntegrationEvent(Guid tripId, string connectionId, Location currentLocation, Action action) : base(tripId, connectionId)
        {
            CurrentLocation = currentLocation;
            Action = action;
        }

        public Action Action { get; }

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

    public class Location
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string Description { get; set; }
    }
}
