using System;
using System.Threading.Tasks;
using AutoMapper;
using Duber.Domain.Trip.Events;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Trip.API.Application.IntegrationEvents;
using Weapsy.Cqrs.Events;

namespace Duber.Trip.API.Application.DomainEventHandlers
{
    public class TripUpdatedDomainEventHandlerAsync : IEventHandlerAsync<TripUpdatedDomainEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly IMapper _mapper;

        public TripUpdatedDomainEventHandlerAsync(IEventBus eventBus, IMapper mapper)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task HandleAsync(TripUpdatedDomainEvent @event)
        {
            var integrationEvent = _mapper.Map<TripUpdatedIntegrationEvent>(@event);
            _eventBus.Publish(integrationEvent); // TODO: make an async Publish method.

            await Task.CompletedTask;
        }
    }
}
