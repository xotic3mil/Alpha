using Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace MVC.Hubs
{
    public class SignalRNotificationHubClient : INotificationHubClient
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationHubClient(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(string group, string method, object notification)
        {
            await _hubContext.Clients.Group(group).SendAsync("ReceiveNotification", notification);
        }

        public async Task NotifyNotificationReadAsync(string group, Guid notificationId)
        {
            await _hubContext.Clients.Group(group).SendAsync("NotificationRead", notificationId);
        }

        public async Task NotifyAllNotificationsReadAsync(string group)
        {
            await _hubContext.Clients.Group(group).SendAsync("AllNotificationsRead");
        }
    }
}
