using System;
using Duber.Infrastructure.EventBus.Events;
using Duber.Trip.API.Application.Model;

namespace Duber.Trip.API.Application.IntegrationEvents
{
    public class TripCreatedIntegrationEvent : IntegrationEvent
    {
        public TripCreatedIntegrationEvent(Guid tripId, int userTripId, int driverId, Location from, Location to, VehicleInformation vehicleInformation, PaymentMethod paymentMethod, string connectionId)
        {
            UserTripId = userTripId;
            DriverId = driverId;
            From = from;
            To = to;
            VehicleInformation = vehicleInformation;
            PaymentMethod = paymentMethod;
            ConnectionId = connectionId;
            TripId = tripId;
        }

        public Guid TripId { get; }

        public int UserTripId { get; }

        public int DriverId { get; }

        public Location From { get; }

        public Location To { get; }

        public VehicleInformation VehicleInformation { get; }

        public PaymentMethod PaymentMethod { get; }

        public string ConnectionId { get; }
    }
}
