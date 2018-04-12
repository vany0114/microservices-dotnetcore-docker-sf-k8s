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
            if (trip == null) throw new InvalidOperationException($"The trip {@event.TripId} doesn't exist. Error trying to update the materialized view.");

            trip.Distance = @event.Distance;
            trip.Duration = @event.Duration;
            trip.Status = @event.Status.Name;
            trip.Started = @event.Started;
            trip.Ended = @event.Ended;

            await _reportingRepository.UpdateTripAsync(trip);
        }
    }
}
