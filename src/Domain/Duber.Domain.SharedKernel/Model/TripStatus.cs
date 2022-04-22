using System;
using System.Collections.Generic;
using System.Linq;
using Duber.Infrastructure.DDD;

namespace Duber.Domain.SharedKernel.Model
{
    public class TripStatus : Enumeration
    {
        public static TripStatus Created = new TripStatus(0, nameof(Created));
        public static TripStatus Accepted = new TripStatus(1, nameof(Accepted));
        public static TripStatus Cancelled = new TripStatus(2, nameof(Cancelled));
        public static TripStatus OnTheWay = new TripStatus(3, nameof(OnTheWay));
        public static TripStatus InCourse = new TripStatus(4, nameof(InCourse));
        public static TripStatus Finished = new TripStatus(5, nameof(Finished));

        protected TripStatus() { }

        public TripStatus(int id, string name)
            : base(id, name)
        {
        }

        public static IEnumerable<TripStatus> List()
        {
            return new[] { Created, Accepted, Cancelled, OnTheWay, InCourse, Finished };
        }

        public static TripStatus FromName(string name)
        {
            var state = List()
                .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
            {
                throw new ArgumentException($"Possible values for TripStatus: {string.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }

        public static TripStatus From(int id)
        {
            var state = List().SingleOrDefault(s => s.Id == id);

            if (state == null)
            {
                throw new ArgumentException($"Possible values for TripStatus: {string.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }
    }
}
