using System;

namespace Duber.Domain.Driver.Exceptions
{
    public class DriverDomainException : Exception
    {
        public DriverDomainException()
        { }

        public DriverDomainException(string message)
            : base(message)
        { }

        public DriverDomainException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
