using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;

namespace MVC.Controllers
{
    public class NotificationController(
        INotificationService notificationService,
        UserManager<UserEntity> userManager) : Controller
    {
        private readonly INotificationService _notificationService = notificationService;
        private readonly UserManager<UserEntity> _userManager = userManager;

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "User not authenticated" });

            try
            {
                if (User.IsInRole("Admin"))
                {
                    var result = await _notificationService.GetUnreadCountForAdminsAsync();
                    return Json(new { success = result.Succeeded, count = result.Succeeded ? result.Result : 0 });
                }
                else if (User.IsInRole("ProjectManager"))
                {
                    var result = await _notificationService.GetUnreadCountForProjectManagersAsync();
                    return Json(new { success = result.Succeeded, count = result.Succeeded ? result.Result : 0 });
                }
                else
                {
                    var result = await _notificationService.GetUnreadCountForUserAsync(Guid.Parse(userId));
                    return Json(new { success = result.Succeeded, count = result.Succeeded ? result.Result : 0 });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "User not authenticated" });

            ProjectManagementResult<IEnumerable<NotificationViewModel>> result;

            if (User.IsInRole("Admin"))
            {
                result = await _notificationService.GetUnreadForAdminsAsAsync<NotificationViewModel>();
            }
            else if (User.IsInRole("ProjectManager"))
            {
                result = await _notificationService.GetUnreadForProjectManagersAsAsync<NotificationViewModel>();
            }
            else
            {
                result = await _notificationService.GetUnreadForUserAsAsync<NotificationViewModel>(Guid.Parse(userId));
            }

            return Json(new
            {
                success = result.Succeeded,
                message = result.Succeeded ? null : result.Error,
                notifications = result.Succeeded ? result.Result : null
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);

            return Json(new
            {
                success = result.Succeeded,
                message = result.Succeeded ? "Notification marked as read" : result.Error
            });
        }
    }
}

