using System;

namespace Duber.Trip.API.Application.Model
{
    public class UpdateTripCommand
    {
        public Guid Id { get; set; }
    }

    public class UpdateCurrentLocationTripCommand : UpdateTripCommand
    {
        public Location CurrentLocation { get; set; }
    }
}
