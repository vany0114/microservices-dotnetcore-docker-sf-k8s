using System.Threading.Tasks;
using Kledex.Commands;

namespace Duber.Domain.Trip.Commands.Handlers
{
    public class CreateTripCommandHandlerAsync : ICommandHandlerAsync<CreateTripCommand>
    {
        public async Task<CommandResponse> HandleAsync(CreateTripCommand command)
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
                command.Model,
                command.ConnectionId);

            await Task.CompletedTask;
            return new CommandResponse
            {
                Events = trip.Events
            };
        }
    }
}
