using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Trip.Notifications.Application.IntegrationEvents.Events;
using Duber.Trip.Notifications.Application.IntegrationEvents.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Duber.Trip.Notifications.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseServiceBroker(this IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<TripCreatedIntegrationEvent, TripCreatedIntegrationEventHandler>();
            eventBus.Subscribe<TripFinishedIntegrationEvent, TripFinishedIntegrationEventHandler>();
            eventBus.Subscribe<TripUpdatedIntegrationEvent, TripUpdatedIntegrationEventHandler>();

            return app;
        }
    }
}
