using System;
using Duber.Domain.Invoice.Events;
using System.Threading.Tasks;
using AutoMapper;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Invoice.API.Application.IntegrationEvents.Events;
using MediatR;

namespace Duber.Invoice.API.Application.DomainEventHandlers
{
    public class InvoiceCreatedDomainEventHandler : IAsyncNotificationHandler<InvoiceCreatedDomainEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly IMapper _mapper;

        public InvoiceCreatedDomainEventHandler(IEventBus eventBus, IMapper mapper)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task Handle(InvoiceCreatedDomainEvent notification)
        {
            // to update the query side (materialized view)
            var integrationEvent = _mapper.Map<InvoiceCreatedIntegrationEvent>(notification);
            _eventBus.Publish(integrationEvent); // TODO: make an async Publish method.

            await Task.CompletedTask;
        }
    }
}
