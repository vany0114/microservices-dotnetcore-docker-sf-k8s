using System;
using System.Threading.Tasks;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.WebSite.Application.IntegrationEvents.Events;
using Duber.WebSite.Infrastructure.Repository;
using Microsoft.Extensions.Logging;

namespace Duber.WebSite.Application.IntegrationEvents.Handlers
{
    public class InvoiceCreatedIntegrationEventHandler: IIntegrationEventHandler<InvoiceCreatedIntegrationEvent>
    {
        private readonly IReportingRepository _reportingRepository;
        private readonly ILogger<InvoiceCreatedIntegrationEventHandler> _logger;

        public InvoiceCreatedIntegrationEventHandler(IReportingRepository reportingRepository, ILogger<InvoiceCreatedIntegrationEventHandler> logger)
        {
            _reportingRepository = reportingRepository ?? throw new ArgumentNullException(nameof(reportingRepository));
            _logger = logger;
        }

        public async Task Handle(InvoiceCreatedIntegrationEvent @event)
        {
            _logger.LogInformation("InvoiceCreatedIntegrationEvent handled");
            var trip = await _reportingRepository.GetTripAsync(@event.TripId);

            // we throw an exception in order to don't send the Acknowledgement to the service bus, probably the consumer read 
            // this message before that the created one.
            if (trip == null)
                throw new InvalidOperationException($"The trip {@event.TripId} doesn't exist. Error trying to update the materialized view.");

            _logger.LogInformation($"Invoice {@event.InvoiceId} for Trip {@event.TripId} has been created.");

            trip.InvoiceId = @event.InvoiceId;
            trip.Fee = @event.Fee;
            trip.Fare = @event.Total - @event.Fee;

            try
            {
                await _reportingRepository.UpdateTripAsync(trip);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error trying to update the Trip: {@event.TripId}", ex);
            }
            finally
            {
                _reportingRepository.Dispose();
            }
        }
    }
}
