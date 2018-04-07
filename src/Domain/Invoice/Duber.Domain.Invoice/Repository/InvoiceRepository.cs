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

        public async Task<Model.Invoice> GetInvoiceAsync(Guid id)
        {
            return await _context.QuerySingleAsync<Model.Invoice>(
                "Select InvoiceId, Fee, Total, PaymentMethodId, TripId, Distance, Duration, Created, TripStatusId From Invoices Where InvoiceId = @InvoiceId",
                new { InvoiceId = id });
        }

        public async Task<Model.Invoice> GetInvoiceByTripAsync(Guid tripId)
        {
            return await _context.QuerySingleAsync<Model.Invoice>(
                "Select InvoiceId, Fee, Total, PaymentMethodId, TripId, Distance, Duration, Created, TripStatusId From Invoices Where TripId = @TripId",
                new { TripId = tripId });
        }

        public async Task<IEnumerable<Model.Invoice>> GetInvoicesAsync()
        {
            return await _context.QueryAsync<Model.Invoice>("Select InvoiceId, Fee, Total, PaymentMethodId, TripId, Distance, Duration, Created, TripStatusId From Invoices");
        }

        public async Task<int> AddInvoiceAsync(Model.Invoice invoice)
        {
            return await _context.ExecuteAsync(
                invoice,
                "Insert Into Invoices(InvoiceId, Fee, Total, PaymentMethodId, Distance, Duration, Created, TripId, TripStatusId) Values(@InvoiceId, @Fee, @Total, @PaymentMethodId, @Distance, @Duration, @Created, @TripId, @TripStatusId)",
                new
                {
                    invoice.InvoiceId,
                    invoice.Fee,
                    invoice.Total,
                    PaymentMethodId = invoice.PaymentMethod.Id,
                    invoice.TripInformation.Distance,
                    invoice.TripInformation.Duration,
                    invoice.Created,
                    TripId = invoice.TripInformation.Id,
                    TripStatusId = invoice.TripInformation.Status.Id
                });
        }
    }
}