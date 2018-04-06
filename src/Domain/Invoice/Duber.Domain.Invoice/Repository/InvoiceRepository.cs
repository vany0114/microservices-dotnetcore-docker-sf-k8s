using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Duber.Domain.Invoice.Persistence;

namespace Duber.Domain.Invoice.Repository
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly IInvoiceContext _context;

        public InvoiceRepository(IInvoiceContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Model.Invoice>> GetInvoicesAsync()
        {
            return await _context.QueryAsync<Model.Invoice>("Select * From Invoices");
        }

        public async Task<int> AddInvoiceAsync(Model.Invoice invoice)
        {
            return await _context.ExecuteAsync(
                invoice,
                "Insert Into Invoices(InvoiceId, Fee, Total, PaymentMethodId, Distance, Duration, Created)",
                new
                {
                    invoice.InvoiceId,
                    invoice.Fee,
                    invoice.Total,
                    PaymentMethodId = invoice.PaymentMethod.Id,
                    invoice.Information.Distance,
                    invoice.Information.Duration,
                    invoice.Created
                });
        }
    }
}