using System;
using System.Threading.Tasks;
using AutoMapper;
using Duber.Domain.Trip.Events;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Infrastructure.EventBus.Idempotency;
using Duber.Trip.API.Application.IntegrationEvents;
using Duber.Trip.API.Application.Model;
using Kledex.Events;
using Microsoft.Extensions.Logging;
using TripStatus = Duber.Domain.SharedKernel.Model.TripStatus;

namespace Duber.Trip.API.Application.DomainEventHandlers
{
    public class TripUpdatedDomainEventHandlerAsync : IEventHandlerAsync<TripUpdatedDomainEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly IMapper _mapper;
        private readonly ILogger<TripUpdatedDomainEventHandlerAsync> _logger;

        public TripUpdatedDomainEventHandlerAsync(IEventBus eventBus, IMapper mapper,
            ILogger<TripUpdatedDomainEventHandlerAsync> logger)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger;
        }

        public async Task HandleAsync(TripUpdatedDomainEvent @event)
        {
            var integrationEvent = _mapper.Map<TripUpdatedIntegrationEvent>(@event);

            // to update the query side (materialized view)
            _logger.LogInformation($"Trip {@event.AggregateRootId} has been updated.");
            _eventBus.Publish(integrationEvent); // TODO: make an async Publish method.

            // events for invoice microservice
            if (@event.Status.Name == TripStatus.Finished.Name)
            {
                if (!@event.Distance.HasValue || !@event.Duration.HasValue || !@event.UserTripId.HasValue)
                    throw new ArgumentException(
                        "Distance, duration and user id are required to trigger a TripFinishedIntegrationEvent");

                _logger.LogInformation($"Trip {@event.AggregateRootId} has finished.");
                var tripFinishedIntegrationEvent = new TripFinishedIntegrationEvent(
                    @event.AggregateRootId,
                    @event.Distance.Value,
                    @event.Duration.Value,
                    new PaymentMethod {Id = @event.PaymentMethod.Id, Name = @event.PaymentMethod.Name},
                    @event.UserTripId.Value,
                    @event.ConnectionId);

                _eventBus.Publish(new IdempotentIntegrationEvent<TripFinishedIntegrationEvent>(tripFinishedIntegrationEvent, @event.AggregateRootId.ToString()));
            }
            else if (@event.Status.Name == TripStatus.Cancelled.Name)
            {
                if (!@event.Duration.HasValue || !@event.UserTripId.HasValue)
                    throw new ArgumentException(
                        "Duration and user id are required to trigger a TripCancelledIntegrationEvent");

                _logger.LogInformation($"Trip {@event.AggregateRootId} has been canceled.");
                var tripCancelledIntegrationEvent = new TripCancelledIntegrationEvent(
                    @event.AggregateRootId,
                    @event.Duration.Value,
                    new PaymentMethod {Id = @event.PaymentMethod.Id, Name = @event.PaymentMethod.Name},
                    @event.UserTripId.Value,
                    @event.ConnectionId);

                _eventBus.Publish(new IdempotentIntegrationEvent<TripCancelledIntegrationEvent>(tripCancelledIntegrationEvent, @event.AggregateRootId.ToString()));
            }

            await Task.CompletedTask;
        }
    }

    public class TripUpdatedIdempotentEventHandler : IdempotentIntegrationEventHandler<TripFinishedIntegrationEvent>
    {
        private readonly ILogger<TripUpdatedIdempotentEventHandler> _logger;

        public TripUpdatedIdempotentEventHandler(IEventBus eventBus, IIdempotencyStoreProvider storeProvider,
            ILogger<TripUpdatedIdempotentEventHandler> logger) :
            base(eventBus, storeProvider)
        {
            _logger = logger;
        }

        protected override void HandleDuplicatedRequest(TripFinishedIntegrationEvent message)
        {
            _logger.LogInformation($"TripFinishedIntegrationEvent was already handled. Trip {message.TripId}.");
            base.HandleDuplicatedRequest(message);
        }
    }
}
