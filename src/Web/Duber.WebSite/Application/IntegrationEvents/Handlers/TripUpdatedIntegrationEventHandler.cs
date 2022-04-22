using System;
using System.Threading.Tasks;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.WebSite.Application.IntegrationEvents.Events;
using Duber.WebSite.Infrastructure.Repository;
using Microsoft.Extensions.Logging;

namespace Duber.WebSite.Application.IntegrationEvents.Handlers
{
    public class TripUpdatedIntegrationEventHandler : IIntegrationEventHandler<TripUpdatedIntegrationEvent>
    {
        private readonly IReportingRepository _reportingRepository;
        private readonly ILogger<TripUpdatedIntegrationEventHandler> _logger;

        public TripUpdatedIntegrationEventHandler(IReportingRepository reportingRepository, ILogger<TripUpdatedIntegrationEventHandler> logger)
        {
            _reportingRepository = reportingRepository ?? throw new ArgumentNullException(nameof(reportingRepository));
            _logger = logger;
        }

        public async Task Handle(TripUpdatedIntegrationEvent @event)
        {
            _logger.LogInformation("TripUpdatedIntegrationEvent handled");
            var trip = await _reportingRepository.GetTripAsync(@event.TripId);

            // we throw an exception in order to don't send the Acknowledgement to the service bus, probably the consumer read the 
            // updated message before that the created one.
            if (trip == null)
                throw new InvalidOperationException($"The trip {@event.TripId} doesn't exist. Error trying to update the materialized view.");

            _logger.LogInformation($"Trip {@event.TripId} has been updated. Status: {@event.Status}");
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
