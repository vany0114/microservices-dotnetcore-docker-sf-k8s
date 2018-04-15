using System;
using System.Threading.Tasks;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.WebSite.Application.IntegrationEvents.Events;
using Duber.WebSite.Infrastructure.Repository;
using TripStatus = Duber.Domain.SharedKernel.Model.TripStatus;

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

            // since it could be read first the finished message and then read an earlier message and set null again this information.
            // consider a separate handler for TripFinishedIntegrationEvent/TripCancelledIntegrationEvent to avoid this
            trip.Distance = trip.Distance ?? @event.Distance; 
            trip.Duration = trip.Duration ?? @event.Duration;
            trip.Status = trip.Status == TripStatus.Finished.Name ? trip.Status : @event.Status.Name;
            
            trip.Started = @event.Started;
            trip.Ended = @event.Ended;

            try
            {
                await _reportingRepository.UpdateTripAsync(trip);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error trying to update the Trip: {@event.TripId}", ex);
            }
        }
    }
}
