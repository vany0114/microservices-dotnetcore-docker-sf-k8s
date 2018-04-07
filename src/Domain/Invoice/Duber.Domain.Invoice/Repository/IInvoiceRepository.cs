using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Duber.Domain.Invoice.Repository
{
    public interface IInvoiceRepository
    {
        Task<Model.Invoice> GetInvoiceAsync(Guid id);

        Task<Model.Invoice> GetInvoiceByTripAsync(Guid tripId);

        Task<IEnumerable<Model.Invoice>> GetInvoicesAsync();

        Task<int> AddInvoiceAsync(Model.Invoice invoice);
    }
}