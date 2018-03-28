using System;
using System.Threading;
using System.Threading.Tasks;

namespace Duber.Infrastructure.Repository.Abstractions
{
    public interface IUnitOfWork : IDisposable
    {        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
