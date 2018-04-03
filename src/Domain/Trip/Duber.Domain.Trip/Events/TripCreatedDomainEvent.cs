using Duber.Domain.Trip.Model;
using Weapsy.Cqrs.Domain;

namespace Duber.Domain.Trip.Events
{
    public class TripCreatedDomainEvent : DomainEvent
    {
        public int UserTripId { get; set; }

        public int DriverId { get; set; }

        public Location From { get; set; }

        public Location To { get; set; }

        public VehicleInformation VehicleInformation { get; set; }
    }
}
