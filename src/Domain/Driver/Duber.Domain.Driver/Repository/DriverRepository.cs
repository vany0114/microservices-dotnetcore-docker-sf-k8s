using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duber.Domain.Driver.Persistence;
using Duber.Infrastructure.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Duber.Domain.Driver.Repository
{
    public class DriverRepository : IDriverRepository
    {
        private readonly DriverContext _context;

        public DriverRepository(DriverContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => _context;

        public async Task<IList<Model.Driver>> GetDriversAsync()
        {
            return await _context.Drivers
                .Include(x => x.Status)
                .Include(x => x.Vehicles)
                .ThenInclude(x => x.Type)
                .ToListAsync();
        }

        public async Task<Model.Driver> GetDriverAsync(int driverId)
        {
            return await _context.Drivers.SingleOrDefaultAsync(x => x.Id == driverId);
        }

        public Model.Driver GetDriver(int driverId)
        {
            return _context.Drivers.SingleOrDefault(x => x.Id == driverId);
        }

        public void Update(Model.Driver driver)
        {
            _context.Entry(driver).State = EntityState.Modified;
        }
    }
}