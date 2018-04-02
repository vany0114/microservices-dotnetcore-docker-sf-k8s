using System;
using System.Threading.Tasks;
using Weapsy.Cqrs.Events;

namespace Duber.Domain.Trip.Events.Handlers
{
    public class TripUpdatedHandlerAsync : IEventHandlerAsync<TripUpdated>
    {
        public async Task HandleAsync(TripUpdated @event)
        {
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
