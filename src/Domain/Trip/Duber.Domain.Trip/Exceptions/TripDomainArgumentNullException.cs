using System;
using System.Collections.Generic;
using System.Text;

namespace Duber.Domain.Trip.Exceptions
{
    public class TripDomainArgumentNullException : ArgumentNullException
    {
        public TripDomainArgumentNullException()
        { }

        public TripDomainArgumentNullException(string message)
            : base(message)
        { }

        public TripDomainArgumentNullException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
