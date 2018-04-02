using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Duber.Domain.Trip.Exceptions;
using Weapsy.Cqrs.Commands;
using Weapsy.Cqrs.Domain;

namespace Duber.Domain.Trip.Commands.Handlers
{
    public class UpdateTripHandlerAsync : ICommandHandlerWithAggregateAsync<UpdatedTrip>
    {
        private readonly IRepository<Model.Trip> _repository;

        public UpdateTripHandlerAsync(IRepository<Model.Trip> repository)
        {
            _repository = repository;
        }

        public async Task<IAggregateRoot> HandleAsync(UpdatedTrip command)
        {
            var trip = await _repository.GetByIdAsync(command.AggregateRootId);

            if (trip == null)
                throw new TripDomainInvalidOperationException("Trip not found.");

            // TODO: consider create a separete command/handler for each action.
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
