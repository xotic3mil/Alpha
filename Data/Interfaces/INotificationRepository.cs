using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<NotificationEntity>> GetUnreadForAdminsAsync();
        Task<IEnumerable<NotificationEntity>> GetUnreadForProjectManagersAsync();
        Task<IEnumerable<NotificationEntity>> GetUnreadForUserAsync(Guid userId);
        Task<int> GetUnreadCountForAdminsAsync();
        Task<int> GetUnreadCountForProjectManagersAsync();
        Task<int> GetUnreadCountForUserAsync(Guid userId);
        Task<NotificationEntity> GetByIdAsync(Guid id);
        Task CreateAsync(NotificationEntity notification);
        Task MarkAllAdminNotificationsAsReadAsync();
        Task MarkAllProjectManagerNotificationsAsReadAsync();
        Task MarkAllUserNotificationsAsReadAsync(Guid userId);
        Task MarkAsReadAsync(Guid id);
        Task DeleteAsync(Guid id);
        Task SaveChangesAsync();
    }
}
