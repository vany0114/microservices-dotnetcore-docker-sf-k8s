using System;
using System.Collections.Generic;
using System.Linq;
using Duber.Infrastructure.DDD;

namespace Duber.Domain.Driver.Model
{
    public class VehicleType : Enumeration
    {
        public static VehicleType Car = new VehicleType(1, nameof(Car));
        public static VehicleType Bike = new VehicleType(2, nameof(Bike));
        public static VehicleType TuckTuck = new VehicleType(3, "Tuck Tuck");

        protected VehicleType() { }

        public VehicleType(int id, string name)
            : base(id, name)
        {
        }

        public static IEnumerable<VehicleType> List()
        {
            return new[] { Car, Bike, TuckTuck };
        }

        public static VehicleType FromName(string name)
        {
            var state = List()
                .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
            {
                throw new ArgumentException($"Possible values for VehicleType: {string.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }

        public static VehicleType From(int id)
        {
            var state = List().SingleOrDefault(s => s.Id == id);

            if (state == null)
            {
                throw new ArgumentException($"Possible values for VehicleType: {string.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }
    }
}
