using System;
using System.Collections.Generic;
using Duber.Infrastructure.DDD;
// ReSharper disable UnusedMember.Local

namespace Duber.Domain.Invoice.Model
{
    public class TripInformation : ValueObject
    {
        public TimeSpan Duration { get; }

        public double Distance { get; }

        private TripInformation() { }

        internal TripInformation(TimeSpan duration, double distance)
        {
            Duration = duration;
            Distance = distance;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Duration;
            yield return Distance;
        }
    }
}
