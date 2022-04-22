using System.Collections.Generic;
using Duber.Infrastructure.DDD;
// ReSharper disable UnusedMember.Local

namespace Duber.Domain.Trip.Model
{
    public class Location : ValueObject
    {
        public double Latitude { get; private set; }

        public double Longitude { get; private set; }

        public string Description { get; private set; }

        public Location(double latitude, double longitude, string description)
        {
            Latitude = latitude;
            Longitude = longitude;
            Description = description;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Latitude;
            yield return Longitude;
        }
    }
}
