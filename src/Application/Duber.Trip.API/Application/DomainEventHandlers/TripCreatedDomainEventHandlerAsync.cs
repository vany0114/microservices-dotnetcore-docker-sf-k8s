using System;
using System.Threading.Tasks;
using AutoMapper;
using Duber.Domain.Trip.Events;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Trip.API.Application.IntegrationEvents;
using Kledex.Events;
using Microsoft.Extensions.Logging;

namespace Duber.Trip.API.Application.DomainEventHandlers
{
    public class TripCreatedDomainEventHandlerAsync : IEventHandlerAsync<TripCreatedDomainEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly IMapper _mapper;
        private readonly ILogger<TripCreatedDomainEventHandlerAsync> _logger;

        public TripCreatedDomainEventHandlerAsync(IEventBus eventBus, IMapper mapper, ILogger<TripCreatedDomainEventHandlerAsync> logger)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger;
        }

        public async Task HandleAsync(TripCreatedDomainEvent @event)
        {
            _logger.LogInformation($"Trip {@event.AggregateRootId} has been created.");
            var integrationEvent = _mapper.Map<TripCreatedIntegrationEvent>(@event);

            // to update the query side (materialized view)
            _eventBus.Publish(integrationEvent); // TODO: make an async Publish method.

            await Task.CompletedTask;
        }
    }
}
