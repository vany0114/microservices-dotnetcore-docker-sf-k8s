using Duber.Infrastructure.DDD;

namespace Duber.Infrastructure.Repository.Abstractions
{
    public interface IRepository<T> where T : IAggregateRoot
    {
        IUnitOfWork UnitOfWork { get; }
    }
}
