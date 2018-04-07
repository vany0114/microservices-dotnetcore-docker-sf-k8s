using Duber.Domain.Invoice.Events;
using System.Threading.Tasks;
using MediatR;

namespace Duber.Invoice.API.Application.DomainEventHandlers
{
    public class InvoiceCreatedDomainEventHandler : IAsyncNotificationHandler<InvoiceCreatedDomainEvent>
    {
        public Task Handle(InvoiceCreatedDomainEvent notification)
        {
            return Task.CompletedTask;
        }
    }
}
