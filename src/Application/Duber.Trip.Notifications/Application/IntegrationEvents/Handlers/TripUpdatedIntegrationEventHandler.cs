using System.Threading.Tasks;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Trip.Notifications.Application.IntegrationEvents.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Duber.Trip.Notifications.Application.IntegrationEvents.Handlers
{
    public class TripUpdatedIntegrationEventHandler : IIntegrationEventHandler<TripUpdatedIntegrationEvent>
    {
        private readonly IHubContext<NotificationsHub> _hubContext;
        private readonly ILogger<TripUpdatedIntegrationEventHandler> _logger;

        public TripUpdatedIntegrationEventHandler(IHubContext<NotificationsHub> hubContext, ILogger<TripUpdatedIntegrationEventHandler> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task Handle(TripUpdatedIntegrationEvent @event)
        {
            _logger.LogInformation($"Trip {@event.TripId} has been updated.");

            if (@event.Action == Action.UpdatedCurrentLocation)
            {
                await _hubContext.Clients.Client(@event.ConnectionId).SendAsync("UpdateCurrentPosition", @event.CurrentLocation);
            }
            else
            {
                await _hubContext.Clients.Client(@event.ConnectionId).SendAsync("NotifyTrip", @event.Action.ToString());
            }
        }
    }
}
