using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Models;

namespace Business.Services
{
    public class NotificationService(INotificationRepository notificationRepository, INotificationHubClient notificationHubClient) : INotificationService
    {
        private readonly INotificationRepository _notificationRepository = notificationRepository;
        private readonly INotificationHubClient _notificationHubClient = notificationHubClient;

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
                    ForProjectManagersOnly = false,
                    RecipientId = null
                };

                await _notificationRepository.CreateAsync(notification);

                var notificationModel = notification.MapTo<Notification>();
                await _notificationHubClient.SendNotificationAsync("admins", "ReceiveNotification", notificationModel);

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
                    ForProjectManagersOnly = true,
                    RecipientId = null
                };

                await _notificationRepository.CreateAsync(notification);

                var notificationModel = notification.MapTo<Notification>();
                await _notificationHubClient.SendNotificationAsync("project-managers", "ReceiveNotification", notificationModel);

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
                    Id = Guid.NewGuid(), 
                    Title = title,
                    Message = message,
                    Type = type,
                    RelatedEntityId = relatedEntityId,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    ForAdminsOnly = false,
                    ForProjectManagersOnly = false, 
                    RecipientId = userId
                };

                await _notificationRepository.CreateAsync(notification);


                var notificationModel = notification.MapTo<Notification>();
                await _notificationHubClient.SendNotificationAsync($"user-{userId}", "ReceiveNotification", notificationModel);

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


        public async Task<ProjectManagementResult> MarkAllAdminNotificationsAsReadAsync()
        {
            try
            {
                await _notificationRepository.MarkAllAdminNotificationsAsReadAsync();
                await _notificationHubClient.NotifyAllNotificationsReadAsync("admins");

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

                await _notificationHubClient.NotifyAllNotificationsReadAsync("project-managers");

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

                await _notificationHubClient.NotifyAllNotificationsReadAsync($"user-{userId}");

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
                var notification = await _notificationRepository.GetByIdAsync(notificationId);
                if (notification == null)
                {
                    return new ProjectManagementResult
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Error = "Notification not found"
                    };
                }

                await _notificationRepository.MarkAsReadAsync(notificationId);

                string group;
                if (notification.ForAdminsOnly)
                {
                    group = "admins";
                }
                else if (notification.ForProjectManagersOnly)
                {
                    group = "project-managers";
                }
                else if (notification.RecipientId.HasValue)
                {
                    group = $"user-{notification.RecipientId}";
                }
                else
                {
                    return new ProjectManagementResult { Succeeded = true, StatusCode = 200 };
                }

                await _notificationHubClient.NotifyNotificationReadAsync(group, notificationId);

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

                await _notificationHubClient.NotifyAllNotificationsReadAsync($"user-{userId}");

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
                var notification = await _notificationRepository.GetByIdAsync(id);
                if (notification == null)
                {
                    return new ProjectManagementResult
                    {
                        Succeeded = false,
                        StatusCode = 404,
                        Error = "Notification not found"
                    };
                }

                await _notificationRepository.MarkAsReadAsync(id);

                string group;
                if (notification.ForAdminsOnly)
                {
                    group = "admins";
                }
                else if (notification.ForProjectManagersOnly)
                {
                    group = "project-managers";
                }
                else if (notification.RecipientId.HasValue)
                {
                    group = $"user-{notification.RecipientId}";
                }
                else
                {
                    return new ProjectManagementResult { Succeeded = true, StatusCode = 200 };
                }

                await _notificationHubClient.NotifyNotificationReadAsync(group, id);

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

                var filteredNotifications = notifications.Where(n => n.Type != "ExcludedType");
                var result = filteredNotifications.Select(n => n.MapTo<Notification>());
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
    }
}