using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Domain.Extensions;
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

            try
            {
                if (User.IsInRole("Admin"))
                {
                    var result = await _notificationService.GetUnreadForAdminsAsync();
                    if (!result.Succeeded)
                        return Json(new { success = false, message = result.Error });

                    var viewModels = result.Result.Select(n => n.MapTo<NotificationViewModel>());
                    return Json(new { success = true, notifications = viewModels });
                }
                else if (User.IsInRole("ProjectManager"))
                {
                    var result = await _notificationService.GetUnreadForProjectManagersAsAsync();
                    if (!result.Succeeded)
                        return Json(new { success = false, message = result.Error });

                    var viewModels = result.Result.Select(n => n.MapTo<NotificationViewModel>());
                    return Json(new { success = true, notifications = viewModels });
                }
                else
                {
                    var result = await _notificationService.GetUnreadForUserAsAsync(Guid.Parse(userId));
                    if (!result.Succeeded)
                        return Json(new { success = false, message = result.Error });

                    var viewModels = result.Result.Select(n => n.MapTo<NotificationViewModel>());
                    return Json(new { success = true, notifications = viewModels });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "User not authenticated" });

            try
            {
                bool isAdmin = User.IsInRole("Admin");
                bool isProjectManager = User.IsInRole("ProjectManager");

                if (isAdmin)
                {
                    await _notificationService.MarkAllAdminNotificationsAsReadAsync();
                }
                else if (isProjectManager)
                {
                    await _notificationService.MarkAllProjectManagerNotificationsAsReadAsync();
                }
                else
                {
                    await _notificationService.MarkAllUserNotificationsAsReadAsync(Guid.Parse(userId));
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            try
            {
                var result = await _notificationService.MarkNotificationAsReadAsync(id);

                if (result.Succeeded)
                {
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = result.Error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}

