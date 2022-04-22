using System;

namespace Duber.Trip.API.Application.Model
{
    public class Trip
    {
        public Guid Id { get; set; }

        public int UserId { get; set; }

        public int DriverId { get; set; }

        public Location From { get; set; }

        public Location To { get; set; }

        public Location CurrentLocation { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Started { get; set; }

        public DateTime? End { get; set; }

        public TripStatus Status { get; set; }

        public VehicleInformation VehicleInformation { get; set; }

        public Rating Rating { get; set; }

        public TimeSpan? Duration { get; set; }

        public double Distance { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
    }

    public class VehicleInformation
    {
        public string Plate { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }
    }

    public class TripStatus
    {
        public string Name { get; set; }

        public int Id { get; set; }
    }

    public class PaymentMethod
    {
        public string Name { get; set; }

        public int Id { get; set; }
    }

    public class Rating
    {
        public int Driver { get; set; }

        public int User { get; set; }
    }
}

