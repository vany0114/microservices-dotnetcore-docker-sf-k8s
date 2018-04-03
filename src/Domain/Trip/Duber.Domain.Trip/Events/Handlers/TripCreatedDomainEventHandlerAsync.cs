using System.Threading.Tasks;
using Weapsy.Cqrs.Events;

namespace Duber.Domain.Trip.Events.Handlers
{
    public class TripCreatedDomainEventHandlerAsync : IEventHandlerAsync<TripCreatedDomainEvent>
    {
        public async Task HandleAsync(TripCreatedDomainEvent @event)
        {
            await Task.CompletedTask;
            // TODO: service bus stuff to notify query side.
        }
    }
}
