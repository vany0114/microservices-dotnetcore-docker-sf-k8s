using System;
using Duber.Domain.SharedKernel.Model;
using Duber.Domain.Trip.Model;
using Weapsy.Cqrs.Domain;

namespace Duber.Domain.Trip.Events
{
    public class TripUpdatedDomainEvent : DomainEvent
    {
        public Action Action { get; set; }

        public TripStatus Status { get; set; }

        public DateTime? Started { get; set; }

        public DateTime? Ended { get; set; }

        public Location CurrentLocation { get; set; }

        public double? Distance { get; set; }
        
        public TimeSpan? Duration { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public int? UserTripId { get; set; }
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
