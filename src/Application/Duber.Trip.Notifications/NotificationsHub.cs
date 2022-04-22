using Microsoft.AspNetCore.SignalR;

namespace Duber.Trip.Notifications
{
    public class NotificationsHub : Hub
    {
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
}
