using Duber.Domain.Trip.Model;
using Weapsy.Cqrs.Domain;

namespace Duber.Domain.Trip.Commands
{
    public class UpdateTripCommand : DomainCommand
    {
        public Action Action { get; set; }

        public Location CurrentLocation { get; set; }
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
