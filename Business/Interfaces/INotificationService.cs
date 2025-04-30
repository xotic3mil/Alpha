using Business.Dtos;
using Domain.Models;


namespace Business.Interfaces
{
    public interface INotificationService
    {
        Task<ProjectManagementResult> CreateAdminNotificationAsync(string title, string message, string type, Guid? relatedEntityId = null);
        Task<ProjectManagementResult> CreateProjectManagerNotificationAsync(string title, string message, string type, Guid? relatedEntityId = null);
        Task<ProjectManagementResult> CreateUserNotificationAsync(Guid userId, string title, string message, string type, Guid? relatedEntityId = null);
        Task<ProjectManagementResult<IEnumerable<Notification>>> GetUnreadForAdminsAsync();
        Task<ProjectManagementResult<IEnumerable<Notification>>> GetUnreadForProjectManagersAsAsync();
        Task<ProjectManagementResult<IEnumerable<Notification>>> GetUnreadForUserAsAsync(Guid userId);
        Task<ProjectManagementResult<int>> GetUnreadCountForAdminsAsync();
        Task<ProjectManagementResult<int>> GetUnreadCountForProjectManagersAsync();
        Task<ProjectManagementResult<int>> GetUnreadCountForUserAsync(Guid userId);
        Task<ProjectManagementResult> MarkAllAdminNotificationsAsReadAsync();
        Task<ProjectManagementResult> MarkAllProjectManagerNotificationsAsReadAsync();
        Task<ProjectManagementResult> MarkAllUserNotificationsAsReadAsync(Guid userId);
        Task<ProjectManagementResult> MarkNotificationAsReadAsync(Guid notificationId);
        Task<ProjectManagementResult> MarkAllNotificationsAsReadAsync(Guid userId);
        Task<ProjectManagementResult> MarkAsReadAsync(Guid id);
        Task<ProjectManagementResult> CreateUserBroadcastNotificationAsync(string title, string message, string type, Guid? relatedEntityId = null);
    }
}
