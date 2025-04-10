using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Domain.Extensions;
using Domain.Models;


namespace Business.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<ProjectManagementResult> CreateAdminNotificationAsync(string title, string message, string type, Guid? relatedEntityId = null)
        {
            try
            {
                var notification = new NotificationEntity
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                    Message = message,
                    Type = type,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    RelatedEntityId = relatedEntityId,
                    ForAdminsOnly = true,             
                    ForProjectManagersOnly = false
                };

                await _notificationRepository.CreateAsync(notification);

                return new ProjectManagementResult
                {
                    Succeeded = true,
                    StatusCode = 201
                };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<ProjectManagementResult> CreateProjectManagerNotificationAsync(string title, string message, string type, Guid? relatedEntityId = null)
        {
            try
            {
                var notification = new NotificationEntity
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                    Message = message,
                    Type = type,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    RelatedEntityId = relatedEntityId,
                    ForAdminsOnly = false, 
                    ForProjectManagersOnly = true
                };

                await _notificationRepository.CreateAsync(notification);

                return new ProjectManagementResult
                {
                    Succeeded = true,
                    StatusCode = 201
                };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<ProjectManagementResult> CreateUserNotificationAsync(Guid userId, string title, string message, string type, Guid? relatedEntityId = null)
        {
            try
            {
                var notification = new NotificationEntity
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    RelatedEntityId = relatedEntityId,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    ForAdminsOnly = false,
                    RecipientId = userId
                };

                await _notificationRepository.CreateAsync(notification);

                return new ProjectManagementResult
                {
                    Succeeded = true,
                    StatusCode = 201
                };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<ProjectManagementResult<IEnumerable<Notification>>> GetUnreadForAdminsAsync()
        {
            try
            {
                var notifications = await _notificationRepository.GetUnreadForAdminsAsync();
                var result = notifications.Select(n => n.MapTo<Notification>());

                return new ProjectManagementResult<IEnumerable<Notification>> { Succeeded = true, StatusCode = 200, Result = result };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<IEnumerable<Notification>> { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<ProjectManagementResult<IEnumerable<Notification>>> GetUnreadForProjectManagersAsAsync()
        {
            try
            {
                var notifications = await _notificationRepository.GetUnreadForProjectManagersAsync();
                var result = notifications.Select(n => n.MapTo<Notification>());
                return new ProjectManagementResult<IEnumerable<Notification>> { Succeeded = true, StatusCode = 200, Result = result };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<IEnumerable<Notification>> { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<ProjectManagementResult<IEnumerable<Notification>>> GetUnreadForUserAsAsync(Guid userId)
        {
            try
            {
                var notifications = await _notificationRepository.GetUnreadForUserAsync(userId);
                var result = notifications.Select(n => n.MapTo<Notification>());
                return new ProjectManagementResult<IEnumerable<Notification>> { Succeeded = true, StatusCode = 200, Result = result };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<IEnumerable<Notification>> { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<ProjectManagementResult<int>> GetUnreadCountForAdminsAsync()
        {
            try
            {
                var count = await _notificationRepository.GetUnreadCountForAdminsAsync();

                return new ProjectManagementResult<int>
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Result = count
                };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<int>
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<ProjectManagementResult<int>> GetUnreadCountForProjectManagersAsync() 
        {
            try
            {
                var count = await _notificationRepository.GetUnreadCountForProjectManagersAsync();
                return new ProjectManagementResult<int>
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Result = count
                };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<int>
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<ProjectManagementResult<int>> GetUnreadCountForUserAsync(Guid userId)
        {
            try
            {
                var count = await _notificationRepository.GetUnreadCountForUserAsync(userId);

                return new ProjectManagementResult<int>
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Result = count
                };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<int>
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }
        public async Task<ProjectManagementResult> MarkAllAdminNotificationsAsReadAsync() 
        {
            try
            {
                await _notificationRepository.MarkAllAdminNotificationsAsReadAsync();
                return new ProjectManagementResult
                {
                    Succeeded = true,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }
        public async Task<ProjectManagementResult> MarkAllProjectManagerNotificationsAsReadAsync() 
        {
            try
            {
                await _notificationRepository.MarkAllProjectManagerNotificationsAsReadAsync();
                return new ProjectManagementResult
                {
                    Succeeded = true,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }
        public async Task<ProjectManagementResult> MarkAllUserNotificationsAsReadAsync(Guid userId)
        {
            try
            {
                await _notificationRepository.MarkAllUserNotificationsAsReadAsync(userId);

                return new ProjectManagementResult
                {
                    Succeeded = true,
                    StatusCode = 200,
                    Message = "Marked all notifications as read"
                };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }

        public async Task<ProjectManagementResult> MarkNotificationAsReadAsync(Guid notificationId) 
        {
            try
            {
                await _notificationRepository.MarkAsReadAsync(notificationId);
                return new ProjectManagementResult { Succeeded = true, StatusCode = 200 };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }

        }

        public async Task<ProjectManagementResult> MarkAllNotificationsAsReadAsync(Guid userId)
        {
            try
            {
                await _notificationRepository.MarkAllUserNotificationsAsReadAsync(userId); 
                return new ProjectManagementResult { Succeeded = true, StatusCode = 200 };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }


        public async Task<ProjectManagementResult> MarkAsReadAsync(Guid id)
        {
            try
            {
                await _notificationRepository.MarkAsReadAsync(id);

                return new ProjectManagementResult
                {
                    Succeeded = true,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult
                {
                    Succeeded = false,
                    StatusCode = 500,
                    Error = ex.Message
                };
            }
        }
    }

}

