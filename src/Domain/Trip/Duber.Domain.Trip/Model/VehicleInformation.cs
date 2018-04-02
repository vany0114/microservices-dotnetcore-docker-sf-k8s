using System;
using System.Collections.Generic;
using Duber.Infrastructure.DDD;
// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable UnusedMember.Local

namespace Duber.Domain.Trip.Model
{
    public class VehicleInformation : ValueObject
    {
        public String Plate { get; private set; }

        public String Brand { get; private set; }

        public String Model { get; private set; }

        private VehicleInformation() { }

        public VehicleInformation(string plate, string brand, string model)
        {
            Plate = plate;
            Brand = brand;
            Model = model;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Plate;
            yield return Brand;
            yield return Model;
        }
    }
}
