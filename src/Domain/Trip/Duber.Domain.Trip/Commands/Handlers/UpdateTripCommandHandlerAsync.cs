using System;
using System.Threading.Tasks;
using Duber.Domain.Trip.Exceptions;
using Weapsy.Cqrs.Commands;
using Weapsy.Cqrs.Domain;

namespace Duber.Domain.Trip.Commands.Handlers
{
    public class UpdateTripCommandHandlerAsync : ICommandHandlerWithAggregateAsync<UpdateTripCommand>
    {
        private readonly IRepository<Model.Trip> _repository;

        public UpdateTripCommandHandlerAsync(IRepository<Model.Trip> repository)
        {
            _repository = repository;
        }

        public async Task<IAggregateRoot> HandleAsync(UpdateTripCommand command)
        {
            var trip = await _repository.GetByIdAsync(command.AggregateRootId);

            if (trip == null)
                throw new TripDomainInvalidOperationException("Trip not found.");

            // TODO: consider creating a separate command/handler for each action to avoid this code smell.
            switch (command.Action)
            {
                case Action.Accept:
                    trip.Accept();
                    break;
                case Action.Start:
                    trip.Start();
                    break;
                case Action.Cancel:
                    trip.Cancel();
                    break;
                case Action.FinishEarlier:
                    trip.FinishEarlier();
                    break;
                case Action.UpdateCurrentLocation:
                    trip.SetCurrentLocation(command.CurrentLocation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await Task.CompletedTask;
            return trip;
        }
    }
}
