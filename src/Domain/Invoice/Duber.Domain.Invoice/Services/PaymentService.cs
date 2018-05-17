using System;
using System.Threading.Tasks;
using Duber.Domain.ACL.Contracts;
using Duber.Domain.Invoice.Exceptions;
using Duber.Domain.Invoice.Repository;

namespace Duber.Domain.Invoice.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPaymentServiceAdapter _paymentServiceAdapter;

        public PaymentService(IPaymentServiceAdapter paymentServiceAdapter, IInvoiceRepository invoiceRepository)
        {
            _paymentServiceAdapter = paymentServiceAdapter ?? throw new InvoiceDomainArgumentNullException(nameof(paymentServiceAdapter));
            _invoiceRepository = invoiceRepository ?? throw new InvoiceDomainArgumentNullException(nameof(invoiceRepository));
        }

        public async Task PerformPayment(Model.Invoice invoice, int userId)
        {
            try
            {
                var paymentInfo = await _paymentServiceAdapter.ProcessPaymentAsync(userId, invoice.InvoiceId.ToString());
                invoice.ProcessPayment(paymentInfo);
                await _invoiceRepository.AddPaymentInfo(invoice);
            }
            finally 
            {
                _invoiceRepository.Dispose();
            }
        }
    }
}