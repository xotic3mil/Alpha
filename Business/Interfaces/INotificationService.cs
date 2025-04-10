using Business.Dtos;
using Domain.Models;


namespace Business.Interfaces
{
    public interface INotificationService
    {
        Task<ProjectManagementResult> CreateAdminNotificationAsync(string title, string message, string type, Guid? relatedEntityId = null);
        Task<ProjectManagementResult> CreateProjectManagerNotificationAsync(string title, string message, string type, Guid? relatedEntityId = null);
        Task<ProjectManagementResult> CreateUserNotificationAsync(Guid userId, string title, string message, string type, Guid? relatedEntityId = null);
        Task<ProjectManagementResult<IEnumerable<T>>> GetUnreadForAdminsAsAsync<T>() where T : class, new();
        Task<ProjectManagementResult<IEnumerable<T>>> GetUnreadForProjectManagersAsAsync<T>() where T : class, new();
        Task<ProjectManagementResult<IEnumerable<T>>> GetUnreadForUserAsAsync<T>(Guid userId) where T : class, new();
        Task<ProjectManagementResult<int>> GetUnreadCountForAdminsAsync();
        Task<ProjectManagementResult<int>> GetUnreadCountForProjectManagersAsync();
        Task<ProjectManagementResult<int>> GetUnreadCountForUserAsync(Guid userId);
        Task<ProjectManagementResult> MarkAsReadAsync(Guid id);
    }
}
