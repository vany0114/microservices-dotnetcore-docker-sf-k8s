using Duber.Infrastructure.EventBus.Events;
using Duber.Trip.API.Application.Model;

namespace Duber.Trip.API.Application.IntegrationEvents
{
    public class TripCreatedIntegrationEvent : IntegrationEvent
    {
        public TripCreatedIntegrationEvent(int userTripId, int driverId, Location from, Location to, VehicleInformation vehicleInformation)
        {
            UserTripId = userTripId;
            DriverId = driverId;
            From = from;
            To = to;
            VehicleInformation = vehicleInformation;
        }

        public int UserTripId { get; }

        public int DriverId { get; }

        public Location From { get; }

        public Location To { get; }

        public VehicleInformation VehicleInformation { get; }
    }

}
