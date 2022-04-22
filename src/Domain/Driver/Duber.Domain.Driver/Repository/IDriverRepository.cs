using System.Collections.Generic;
using System.Threading.Tasks;
using Duber.Infrastructure.Repository.Abstractions;

namespace Duber.Domain.Driver.Repository
{
    public interface IDriverRepository : IRepository<Model.Driver>
    {
        Model.Driver GetDriver(int driverId);

        Task<IList<Model.Driver>> GetDriversAsync();

        Task<Model.Driver> GetDriverAsync(int driverId);

        void Update(Model.Driver driver);
    }
}