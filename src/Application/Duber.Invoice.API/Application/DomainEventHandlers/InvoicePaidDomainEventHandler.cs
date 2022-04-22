using System;
using System.Threading.Tasks;
using AutoMapper;
using Duber.Domain.Invoice.Events;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Invoice.API.Application.IntegrationEvents.Events;
using MediatR;

namespace Duber.Invoice.API.Application.DomainEventHandlers
{
    public class InvoicePaidDomainEventHandler : IAsyncNotificationHandler<InvoicePaidDomainEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly IMapper _mapper;

        public InvoicePaidDomainEventHandler(IEventBus eventBus, IMapper mapper)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task Handle(InvoicePaidDomainEvent notification)
        {
            // to update the query side (materialized view)
            var integrationEvent = _mapper.Map<InvoicePaidIntegrationEvent>(notification);
            _eventBus.Publish(integrationEvent); // TODO: make an async Publish method.

            await Task.CompletedTask;
        }
    }
}
