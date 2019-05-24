using System;
using System.Threading.Tasks;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.WebSite.Application.IntegrationEvents.Events;
using Duber.WebSite.Infrastructure.Repository;

namespace Duber.WebSite.Application.IntegrationEvents.Handlers
{
    public class TripUpdatedIntegrationEventHandler : IIntegrationEventHandler<TripUpdatedIntegrationEvent>
    {
        private readonly IReportingRepository _reportingRepository;

        public TripUpdatedIntegrationEventHandler(IReportingRepository reportingRepository)
        {
            _reportingRepository = reportingRepository ?? throw new ArgumentNullException(nameof(reportingRepository));
        }

        public async Task Handle(TripUpdatedIntegrationEvent @event)
        {
            var trip = await _reportingRepository.GetTripAsync(@event.TripId);

            // we throw an exception in order to don't send the Acknowledgement to the service bus, probably the consumer read the 
            // updated message before that the created one.
            if (trip == null)
                throw new InvalidOperationException($"The trip {@event.TripId} doesn't exist. Error trying to update the materialized view.");

            if (trip.Status == "Finished") return;

            trip.Distance = @event.Distance;
            trip.Duration = @event.Duration;
            trip.Status = @event.Status.Name;
            trip.Started = @event.Started;
            trip.Ended = @event.Ended;

            try
            {
                await _reportingRepository.UpdateTripAsync(trip);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error trying to update the Trip: {@event.TripId} Trip status: {trip.Status}", ex);
            }
            finally
            {
                _reportingRepository.Dispose();
            }
        }
    }
}
