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
                    Title = title,
                    Message = message,
                    Type = type,
                    RelatedEntityId = relatedEntityId,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    ForAdminsOnly = true
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
                    Title = title,
                    Message = message,
                    Type = type,
                    RelatedEntityId = relatedEntityId,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
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

        public async Task<ProjectManagementResult<IEnumerable<T>>> GetUnreadForAdminsAsAsync<T>() where T : class, new()
        {
            try
            {
                var notifications = await _notificationRepository.GetUnreadForAdminsAsync();
                var result = notifications.Select(n => n.MapTo<T>());

                return new ProjectManagementResult<IEnumerable<T>> { Succeeded = true, StatusCode = 200, Result = result };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<IEnumerable<T>> { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<ProjectManagementResult<IEnumerable<T>>> GetUnreadForProjectManagersAsAsync<T>() where T : class, new() 
        {
            try
            {
                var notifications = await _notificationRepository.GetUnreadForProjectManagersAsync();
                var result = notifications.Select(n => n.MapTo<T>());
                return new ProjectManagementResult<IEnumerable<T>> { Succeeded = true, StatusCode = 200, Result = result };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<IEnumerable<T>> { Succeeded = false, StatusCode = 500, Error = ex.Message };
            }
        }

        public async Task<ProjectManagementResult<IEnumerable<T>>> GetUnreadForUserAsAsync<T>(Guid userId) where T : class, new()
        {
            try
            {
                var notifications = await _notificationRepository.GetUnreadForUserAsync(userId);
                var result = notifications.Select(n => n.MapTo<T>());
                return new ProjectManagementResult<IEnumerable<T>> { Succeeded = true, StatusCode = 200, Result = result };
            }
            catch (Exception ex)
            {
                return new ProjectManagementResult<IEnumerable<T>> { Succeeded = false, StatusCode = 500, Error = ex.Message };
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

