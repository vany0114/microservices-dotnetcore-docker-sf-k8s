using System.Threading.Tasks;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Trip.Notifications.Application.IntegrationEvents.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Duber.Trip.Notifications.Application.IntegrationEvents.Handlers
{
    public class TripFinishedIntegrationEventHandler : IIntegrationEventHandler<TripFinishedIntegrationEvent>
    {
        private readonly IHubContext<NotificationsHub> _hubContext;
        private readonly ILogger<TripFinishedIntegrationEventHandler> _logger;

        public TripFinishedIntegrationEventHandler(IHubContext<NotificationsHub> hubContext, ILogger<TripFinishedIntegrationEventHandler> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task Handle(TripFinishedIntegrationEvent @event)
        {
            _logger.LogInformation($"Trip {@event.TripId} has finished.");
            await _hubContext.Clients.Client(@event.ConnectionId).SendAsync("NotifyTrip", "Finished");
        }
    }
}
