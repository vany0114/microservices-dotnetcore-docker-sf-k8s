using System;

namespace Duber.Domain.Invoice.Exceptions
{
    public class InvoiceDomainArgumentNullException : ArgumentNullException
    {
        public InvoiceDomainArgumentNullException()
        { }

        public InvoiceDomainArgumentNullException(string message)
            : base(message)
        { }

        public InvoiceDomainArgumentNullException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
