using System;
using System.Collections.Generic;
using System.Text;

namespace Duber.Domain.Trip.Exceptions
{
    public class TripDomainInvalidOperationException : InvalidOperationException
    {
        public TripDomainInvalidOperationException()
        { }

        public TripDomainInvalidOperationException(string message)
            : base(message)
        { }

        public TripDomainInvalidOperationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
