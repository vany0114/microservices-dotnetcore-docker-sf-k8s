using System;
using System.Collections.Generic;
using System.Linq;
using Duber.Infrastructure.DDD;

namespace Duber.Domain.Driver.Model
{
    public class DriverStatus : Enumeration
    {
        public static DriverStatus Available = new DriverStatus(1, nameof(Available));
        public static DriverStatus Busy = new DriverStatus(2, nameof(Busy));
        public static DriverStatus Inactive = new DriverStatus(3, nameof(Inactive));
        public static DriverStatus Active = new DriverStatus(4, nameof(Active));

        protected DriverStatus() { }

        public DriverStatus(int id, string name)
            : base(id, name)
        {
        }

        public static IEnumerable<DriverStatus> List()
        {
            return new[] { Available, Busy, Inactive, Active };
        }

        public static DriverStatus FromName(string name)
        {
            var state = List()
                .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
            {
                throw new ArgumentException($"Possible values for DriverStatus: {string.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }

        public static DriverStatus From(int id)
        {
            var state = List().SingleOrDefault(s => s.Id == id);

            if (state == null)
            {
                throw new ArgumentException($"Possible values for DriverStatus: {string.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }
    }
}
