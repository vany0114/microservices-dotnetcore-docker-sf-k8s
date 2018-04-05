using System.Threading.Tasks;
using Weapsy.Cqrs.Commands;
using Weapsy.Cqrs.Domain;

namespace Duber.Domain.Trip.Commands.Handlers
{
    public class CreateTripCommandHandlerAsync : ICommandHandlerWithAggregateAsync<CreateTripCommand>
    {
        public async Task<IAggregateRoot> HandleAsync(CreateTripCommand command)
        {
            var trip = new Model.Trip(
                command.AggregateRootId,
                command.UserTripId,
                command.DriverId,
                command.From,
                command.To,
                command.PaymentMethod,
                command.Plate,
                command.Brand,
                command.Model);
            
            await Task.CompletedTask;
            return trip;
        }
    }
}
