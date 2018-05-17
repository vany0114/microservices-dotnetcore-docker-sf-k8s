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
                "Select i.InvoiceId, i.Fee, i.Total, i.PaymentMethodId, i.TripId, i.Distance, i.Duration, i.Created, i.TripStatusId, p.Status, p.CardNumber, p.CardType, p.UserId " +
                "From Invoices i LEFT JOIN" +
                "   PaymentsInfo p ON p.InvoiceId = i.InvoiceId " +
                "Where i.InvoiceId = @InvoiceId",
                new { InvoiceId = id });
        }

        public async Task<Model.Invoice> GetInvoiceByTripAsync(Guid tripId)
        {
            return await _context.QuerySingleAsync<Model.Invoice>(
                "Select i.InvoiceId, i.Fee, i.Total, i.PaymentMethodId, i.TripId, i.Distance, i.Duration, i.Created, i.TripStatusId, p.Status, p.CardNumber, p.CardType, p.UserId " +
                "From Invoices i LEFT JOIN" +
                "   PaymentsInfo p ON p.InvoiceId = i.InvoiceId " +
                "Where i.TripId = @TripId",
                new { TripId = tripId });
        }

        public async Task<IEnumerable<Model.Invoice>> GetInvoicesAsync()
        {
            return await _context.QueryAsync<Model.Invoice>(
                "Select i.InvoiceId, i.Fee, i.Total, i.PaymentMethodId, i.TripId, i.Distance, i.Duration, i.Created, i.TripStatusId, p.Status, p.CardNumber, p.CardType, p.UserId " +
                "From Invoices i LEFT JOIN" +
                "   PaymentsInfo p ON p.InvoiceId = i.InvoiceId ");
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

        public async Task<int> AddPaymentInfo(Model.Invoice invoice)
        {
            return await _context.ExecuteAsync(
                invoice,
                "Insert Into PaymentsInfo(Status, CardNumber, CardType, InvoiceId, UserId) Values(@Status, @CardNumber, @CardType, @InvoiceId, @UserId)",
                new
                {
                    invoice.PaymentInfo.Status,
                    invoice.PaymentInfo.CardNumber,
                    invoice.PaymentInfo.CardType,
                    invoice.InvoiceId,
                    invoice.PaymentInfo.UserId,
                });
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}