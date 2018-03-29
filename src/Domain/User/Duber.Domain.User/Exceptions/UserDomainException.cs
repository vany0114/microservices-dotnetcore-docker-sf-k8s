using System;

namespace Duber.Domain.User.Exceptions
{
    public class UserDomainException : Exception
    {
        public UserDomainException()
        { }

        public UserDomainException(string message)
            : base(message)
        { }

        public UserDomainException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
