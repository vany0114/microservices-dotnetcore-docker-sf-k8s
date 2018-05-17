using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Duber.WebSite.Models;

namespace Duber.WebSite.Infrastructure.Repository
{
    public interface IReportingRepository : IDisposable
    {
        Trip GetTrip(Guid tripId);

        void AddTrip(Trip trip);

        void UpdateTrip(Trip trip);

        Task AddTripAsync(Trip trip);

        Task UpdateTripAsync(Trip trip);

        Task<IList<Trip>> GetTripsAsync();

        Task<Trip> GetTripAsync(Guid tripId);

        Task<IList<Trip>> GetTripsByUserAsync(int userId);

        Task<IList<Trip>> GetTripsByDriverAsync(int driverid);
    }
}