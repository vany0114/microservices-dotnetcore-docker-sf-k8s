﻿using Duber.Domain.SharedKernel.Model;
using Duber.Domain.Trip.Model;
using OpenCqrs.Domain;

namespace Duber.Domain.Trip.Commands
{
    public class CreateTripCommand : DomainCommand<Model.Trip>
    {
        public int UserTripId { get; set; }

        public int DriverId { get; set; }

        public Location From { get; set; }

        public Location To { get; set; }

        public string Plate { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public string ConnectionId { get; set; }
    }
}
