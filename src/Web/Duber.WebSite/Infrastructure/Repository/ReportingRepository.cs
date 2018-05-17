using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duber.WebSite.Infrastructure.Persistence;
using Duber.WebSite.Models;
using Microsoft.EntityFrameworkCore;

namespace Duber.WebSite.Infrastructure.Repository
{
    public class ReportingRepository : IReportingRepository
    {
        private readonly ReportingContext _reportingContext;

        public ReportingRepository(ReportingContext reportingContext)
        {
            _reportingContext = reportingContext ?? throw new ArgumentNullException(nameof(reportingContext));
        }

        public async Task AddTripAsync(Trip trip)
        {
            _reportingContext.Trips.Add(trip);
            await _reportingContext.SaveChangesAsync();
        }

        public void AddTrip(Trip trip)
        {
            _reportingContext.Trips.Add(trip);
            _reportingContext.SaveChanges();
        }

        public void UpdateTrip(Trip trip)
        {
            _reportingContext.Attach(trip);
            _reportingContext.SaveChanges();
        }

        public async Task UpdateTripAsync(Trip trip)
        {
            _reportingContext.Attach(trip);
            await _reportingContext.SaveChangesAsync();
        }

        public async Task<IList<Trip>> GetTripsAsync()
        {
            return await _reportingContext.Trips.ToListAsync();
        }

        public async Task<Trip> GetTripAsync(Guid tripId)
        {
            return await _reportingContext.Trips.SingleOrDefaultAsync(x => x.Id == tripId);
        }

        public Trip GetTrip(Guid tripId)
        {
            return _reportingContext.Trips.SingleOrDefault(x => x.Id == tripId);
        }

        public async Task<IList<Trip>> GetTripsByUserAsync(int userId)
        {
            return await _reportingContext.Trips
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }

        public async Task<IList<Trip>> GetTripsByDriverAsync(int driverid)
        {
            return await _reportingContext.Trips
                .Where(x => x.DriverId == driverid)
                .OrderByDescending(x => x.Created)
                .ToListAsync();
        }

        public void Dispose()
        {
            _reportingContext?.Dispose();
        }
    }
}