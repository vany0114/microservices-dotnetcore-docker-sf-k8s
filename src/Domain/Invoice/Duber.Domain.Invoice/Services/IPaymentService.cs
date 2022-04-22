using System.Threading.Tasks;
using Duber.Domain.SharedKernel.Model;

namespace Duber.Domain.Invoice.Services
{
    public interface IPaymentService
    {
        Task PerformPayment(Model.Invoice invoice, int userId);
    }
}