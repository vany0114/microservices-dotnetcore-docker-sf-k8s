using System.Threading.Tasks;
using Weapsy.Cqrs.Events;

namespace Duber.Domain.Trip.Events.Handlers
{
    public class TripCreatedHandlerAsync : IEventHandlerAsync<TripCreated>
    {
        public async Task HandleAsync(TripCreated @event)
        {
            await Task.CompletedTask;
            // TODO: service bus stuff to notify query side.
        }
    }
}
