using System;
using System.Threading.Tasks;
using Weapsy.Cqrs.Events;

namespace Duber.Domain.Trip.Events.Handlers
{
    public class TripUpdatedDomainEventHandlerAsync : IEventHandlerAsync<TripUpdatedDomainEvent>
    {
        public async Task HandleAsync(TripUpdatedDomainEvent @event)
        {
            // TODO: consider create a separete event/handler for each action to avoid this code smell.
            switch (@event.Action)
            {
                case Action.Accepted:
                    break;
                case Action.Started:
                    break;
                case Action.Cancelled:
                    break;
                case Action.FinishedEarlier:
                    break;
                case Action.UpdatedCurrentLocation:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await Task.CompletedTask;
            // TODO: service bus stuff to notify query side.
        }
    }
}
