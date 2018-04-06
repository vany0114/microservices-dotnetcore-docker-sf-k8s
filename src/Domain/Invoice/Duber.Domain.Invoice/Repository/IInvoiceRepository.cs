using System.Collections.Generic;
using System.Threading.Tasks;

namespace Duber.Domain.Invoice.Repository
{
    public interface IInvoiceRepository
    {
        Task<IEnumerable<Model.Invoice>> GetInvoicesAsync();

        Task<int> AddInvoiceAsync(Model.Invoice invoice);
    }
}