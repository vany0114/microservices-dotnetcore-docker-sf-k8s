using System.Threading.Tasks;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Trip.Notifications.Application.IntegrationEvents.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Duber.Trip.Notifications.Application.IntegrationEvents.Handlers
{
    public class TripCreatedIntegrationEventHandler : IIntegrationEventHandler<TripCreatedIntegrationEvent>
    {
        private readonly IHubContext<NotificationsHub> _hubContext;
        private readonly ILogger<TripCreatedIntegrationEventHandler> _logger;

        public TripCreatedIntegrationEventHandler(IHubContext<NotificationsHub> hubContext, ILogger<TripCreatedIntegrationEventHandler> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task Handle(TripCreatedIntegrationEvent @event)
        {
            _logger.LogInformation($"Trip {@event.TripId} has been created.");
            await _hubContext.Clients.Client(@event.ConnectionId).SendAsync("NotifyTrip", "Created");
        }
    }
}
