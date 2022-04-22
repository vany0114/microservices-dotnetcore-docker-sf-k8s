using System;
using Duber.Domain.Invoice.Model;

namespace Duber.Domain.Invoice.Extensions
{
    public static class TripInformationExtensions
    {
        public static double DistanceToKilometers(this TripInformation tripInformation)
        {
            return tripInformation.Distance / 1000;
        }

        public static double DurationToMinutes(this TripInformation tripInformation)
        {
            return tripInformation.Duration.TotalMinutes;
        }
    }
}
