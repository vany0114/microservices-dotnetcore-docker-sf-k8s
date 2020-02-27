using Microsoft.AspNetCore.SignalR;

namespace Duber.WebSite.Hubs
{
    public class TripHub : Hub
    {
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
}
