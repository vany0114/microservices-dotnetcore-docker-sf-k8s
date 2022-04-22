using System;

namespace Duber.Domain.Invoice.Exceptions
{
    public class InvoiceDomainInvalidOperationException : InvalidOperationException
    {
        public InvoiceDomainInvalidOperationException()
        { }

        public InvoiceDomainInvalidOperationException(string message)
            : base(message)
        { }

        public InvoiceDomainInvalidOperationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
