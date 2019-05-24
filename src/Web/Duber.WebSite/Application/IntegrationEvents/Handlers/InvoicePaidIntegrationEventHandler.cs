using System;
using System.Threading.Tasks;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.WebSite.Application.IntegrationEvents.Events;
using Duber.WebSite.Infrastructure.Repository;

namespace Duber.WebSite.Application.IntegrationEvents.Handlers
{
    public class InvoicePaidIntegrationEventHandler : IIntegrationEventHandler<InvoicePaidIntegrationEvent>
    {
        private readonly IReportingRepository _reportingRepository;

        public InvoicePaidIntegrationEventHandler(IReportingRepository reportingRepository)
        {
            _reportingRepository = reportingRepository ?? throw new ArgumentNullException(nameof(reportingRepository));
        }

        public async Task Handle(InvoicePaidIntegrationEvent @event)
        {
            var trip = await _reportingRepository.GetTripAsync(@event.TripId);

            // we throw an exception in order to don't send the Acknowledgement to the service bus, probably the consumer read 
            // this message before that the created one.
            if (trip == null)
                throw new InvalidOperationException($"The trip {@event.TripId} doesn't exist. Error trying to update the materialized view.");

            trip.CardNumber = @event.CardNumber;
            trip.CardType = @event.CardType;
            trip.PaymentStatus = @event.Status == PaymentStatus.Accepted ? nameof(PaymentStatus.Accepted) : nameof(PaymentStatus.Rejected);

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
