using System;
using Duber.Infrastructure.EventBus.Events;

namespace Duber.WebSite.Application.IntegrationEvents.Events
{
    public class TripCreatedIntegrationEvent : IntegrationEvent
    {
        public TripCreatedIntegrationEvent(Guid tripId, int userTripId, int driverId, Location from, Location to, VehicleInformation vehicleInformation, PaymentMethod paymentMethod)
        {
            UserTripId = userTripId;
            DriverId = driverId;
            From = from;
            To = to;
            VehicleInformation = vehicleInformation;
            PaymentMethod = paymentMethod;
            TripId = tripId;
        }

        public Guid TripId { get; }

        public int UserTripId { get; }

        public int DriverId { get; }

        public Location From { get; }

        public Location To { get; }

        public VehicleInformation VehicleInformation { get; }

        public PaymentMethod PaymentMethod { get; }
    }

    public class VehicleInformation
    {
        public string Plate { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }
    }

    public class PaymentMethod
    {
        public string Name { get; set; }

        public int Id { get; set; }
    }

    public class Location
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string Description { get; set; }
    }
}
