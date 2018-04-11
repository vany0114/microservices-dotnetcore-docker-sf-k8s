using System.Threading.Tasks;
using Duber.Domain.SharedKernel.Model;

namespace Duber.Domain.ACL.Contracts
{
    public interface IPaymentServiceAdapter
    {
        Task<PaymentInfo> ProcessPaymentAsync(int userId, string reference);
    }
}