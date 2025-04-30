namespace Domain.Interfaces
{
    public interface INotificationHubClient
    {
        Task SendNotificationAsync(string group, string method, object notification);
        Task NotifyNotificationReadAsync(string group, Guid notificationId);
        Task NotifyAllNotificationsReadAsync(string group);
        Task SendBroadcastNotificationAsync(string method, object arg);
    }
}
