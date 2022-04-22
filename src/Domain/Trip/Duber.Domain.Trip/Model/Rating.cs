using System.Collections.Generic;
using Duber.Infrastructure.DDD;
// ReSharper disable UnusedMember.Local

namespace Duber.Domain.Trip.Model
{
    public class Rating : ValueObject
    {
        public int Driver { get; private set; }

        public int User { get; private set; }

        private Rating() { }

        public Rating(int driver, int user)
        {
            Driver = driver;
            User = user;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Driver;
            yield return User;
        }
    }
}
