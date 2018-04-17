using System.Threading.Tasks;
using Duber.WebSite.Models;
using Microsoft.AspNetCore.SignalR;

namespace Duber.WebSite.Hubs
{
    public class TripHub : Hub
    {
        public Task UpdateCurrentPosition(LocationModel position)
        {
            return Clients.All.SendAsync("UpdateCurrentPosition", position);
        }

        public Task NotifyTrip(string message)
        {
            return Clients.All.SendAsync("NotifyTrip", message);
        }
    }
}
