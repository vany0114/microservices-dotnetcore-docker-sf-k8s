using System.Collections.Generic;
using System.Threading.Tasks;
using Duber.Infrastructure.Repository.Abstractions;

namespace Duber.Domain.Driver.Repository
{
    public interface IDriverRepository : IRepository<Model.Driver>
    {
        Task<IList<Model.Driver>> GetDriversAsync();

        void Update(Model.Driver driver);
    }
}