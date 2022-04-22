using System.Threading.Tasks;

namespace Duber.Infrastructure.EventBus.Idempotency
{
    public interface IIdempotencyStoreProvider
    {
        Task SaveAsync(IdempotentMessage message);

        Task<bool> ExistsAsync(string id);
    }
}
