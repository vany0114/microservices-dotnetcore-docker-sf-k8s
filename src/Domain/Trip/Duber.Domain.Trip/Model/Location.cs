using System.Collections.Generic;
using Duber.Infrastructure.DDD;
// ReSharper disable UnusedMember.Local

namespace Duber.Domain.Trip.Model
{
    public class Location : ValueObject
    {
        public double Latitude { get; private set; }

        public double Longitude { get; private set; }

        private Location() { }

        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Latitude;
            yield return Longitude;
        }

    }
}
