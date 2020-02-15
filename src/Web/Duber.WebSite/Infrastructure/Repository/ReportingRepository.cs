using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duber.Infrastructure.Resilience.Abstractions;
using Duber.WebSite.Infrastructure.Persistence;
using Duber.WebSite.Models;
using Microsoft.EntityFrameworkCore;

namespace Duber.WebSite.Infrastructure.Repository
{
    public class ReportingRepository : IReportingRepository
    {
        private readonly ReportingContext _reportingContext;
        private readonly IPolicyAsyncExecutor _resilientAsyncSqlExecutor;
        private readonly IPolicySyncExecutor _resilientSyncSqlExecutor;

        public ReportingRepository(ReportingContext reportingContext, IPolicyAsyncExecutor resilientAsyncSqlExecutor, IPolicySyncExecutor resilientSyncSqlExecutor)
        {
            _reportingContext = reportingContext ?? throw new ArgumentNullException(nameof(reportingContext));
            _resilientAsyncSqlExecutor = resilientAsyncSqlExecutor ?? throw new ArgumentNullException(nameof(resilientAsyncSqlExecutor));
            _resilientSyncSqlExecutor = resilientSyncSqlExecutor ?? throw new ArgumentNullException(nameof(resilientSyncSqlExecutor));
        }

        public async Task AddTripAsync(Trip trip)
        {
            _reportingContext.Trips.Add(trip);
            await _resilientAsyncSqlExecutor.ExecuteAsync(async () => await _reportingContext.SaveChangesAsync());
        }

        public void AddTrip(Trip trip)
        {
            _reportingContext.Trips.Add(trip);
            _resilientSyncSqlExecutor.Execute(() => _reportingContext.SaveChanges());
        }

        public void UpdateTrip(Trip trip)
        {
            _reportingContext.Attach(trip);
            _resilientSyncSqlExecutor.Execute(() => _reportingContext.SaveChanges());
        }

        public async Task UpdateTripAsync(Trip trip)
        {
            _reportingContext.Attach(trip);
            await _resilientAsyncSqlExecutor.ExecuteAsync(async () => await _reportingContext.SaveChangesAsync());
        }

        public async Task<IList<Trip>> GetTripsAsync()
        {
            return await _resilientAsyncSqlExecutor.ExecuteAsync(async () => await _reportingContext.Trips.ToListAsync());
        }

        public async Task<Trip> GetTripAsync(Guid tripId)
        {
            return await _resilientAsyncSqlExecutor.ExecuteAsync(async () =>
                await _reportingContext.Trips.SingleOrDefaultAsync(x => x.Id == tripId));
        }

        public Trip GetTrip(Guid tripId)
        {
            return _resilientSyncSqlExecutor.Execute(() => _reportingContext.Trips.SingleOrDefault(x => x.Id == tripId));
        }

        public async Task<IList<Trip>> GetTripsByUserAsync(int userId)
        {
            return await _resilientAsyncSqlExecutor.ExecuteAsync(async () =>
                await _reportingContext.Trips
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.Created)
                    .ToListAsync());
        }

        public async Task<IList<Trip>> GetTripsByDriverAsync(int driverid)
        {
            return await _resilientAsyncSqlExecutor.ExecuteAsync(async () =>
                await _reportingContext.Trips
                    .Where(x => x.DriverId == driverid)
                    .OrderByDescending(x => x.Created)
                    .ToListAsync());
        }

        public void Dispose()
        {
            _reportingContext?.Dispose();
        }
    }
}