using Duber.Domain.Trip.Model;
using Kledex.Domain;

namespace Duber.Domain.Trip.Commands
{
    public class UpdateTripCommand : DomainCommand<Model.Trip>
    {
        public Action Action { get; set; }

        public Location CurrentLocation { get; set; }

        public string ConnectionId { get; set; }
    }

    public enum Action
    {
        Accept = 1,
        Start = 2,
        Cancel = 3,
        FinishEarlier = 4,
        UpdateCurrentLocation = 5
    }
}
