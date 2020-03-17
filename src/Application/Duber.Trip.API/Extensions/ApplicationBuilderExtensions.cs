using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Infrastructure.EventBus.Idempotency;
using Duber.Trip.API.Application.DomainEventHandlers;
using Duber.Trip.API.Application.IntegrationEvents;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Duber.Trip.API.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseIdempotency(this IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<IdempotentIntegrationEvent<TripFinishedIntegrationEvent>, TripUpdatedIdempotentEventHandler>();
        }
    }
}
