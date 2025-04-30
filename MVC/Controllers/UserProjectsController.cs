using Business.Interfaces;
using Data.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers
{
    public class UserProjectsController(IProjectMembershipService projectMembershipService, UserManager<UserEntity> userManager, INotificationService notificationService) : Controller
    {
        private readonly IProjectMembershipService _projectMembershipService = projectMembershipService;
        private readonly INotificationService _notificationService = notificationService;
        private readonly UserManager<UserEntity> _userManager = userManager;

        [HttpGet]
        public async Task<IActionResult> GetProjectMembers(Guid projectId)
        {
            var result = await projectMembershipService.GetProjectMembersAsync(projectId);
            if (!result.Succeeded)
                return Json(new { success = false, message = result.Error });

            return Json(new
            {
                success = true,
                members = result.Result.Select(m => new {
                    id = m.Id,
                    firstName = m.FirstName,
                    lastName = m.LastName,
                    email = m.Email,
                    avatarUrl = m.AvatarUrl ?? "/images/avatar-template-1.svg"
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableUsers(Guid projectId)
        {
            var result = await projectMembershipService.GetAvailableUsersForProjectAsync(projectId);
            if (!result.Succeeded)
                return Json(new { success = false, message = result.Error });

            return Json(new
            {
                success = true,
                users = result.Result.Select(u => new {
                    id = u.Id,
                    name = $"{u.FirstName} {u.LastName}",
                    email = u.Email,
                    avatarUrl = u.AvatarUrl ?? "/images/avatar-template-1.svg"
                })
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserToProject(Guid projectId, Guid userId)
        {
            var result = await projectMembershipService.AddUserToProjectAsync(projectId, userId);
            return Json(new { success = result.Succeeded, message = result.Succeeded ? "User added successfully" : result.Error });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserFromProject(Guid projectId, Guid userId)
        {
            var result = await projectMembershipService.RemoveUserFromProjectAsync(projectId, userId);
            return Json(new { success = result.Succeeded, message = result.Succeeded ? "User removed successfully" : result.Error });
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingRequests(Guid projectId)
        {
            var result = await projectMembershipService.GetPendingRequestsForProjectAsync(projectId);
            if (!result.Succeeded)
                return Json(new { success = false, message = result.Error });

            return Json(new
            {
                success = true,
                requests = result.Result.Select(r => new {
                    id = r.Id,
                    projectId = r.ProjectId,
                    projectName = r.ProjectName,
                    userId = r.UserId,
                    userName = r.UserName,
                    userEmail = r.UserEmail,
                    userAvatarUrl = r.UserAvatarUrl ?? "/images/avatar-template-1.svg",
                    requestDate = r.RequestDate,
                    message = r.Message
                })
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> ApproveRequest(Guid requestId)
        {
            var result = await projectMembershipService.ApproveProjectRequestAsync(requestId);
            return Json(new { success = result.Succeeded, message = result.Succeeded ? "Request approved" : result.Error });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> RejectRequest(Guid requestId)
        {
            var result = await projectMembershipService.RejectProjectRequestAsync(requestId);
            return Json(new { success = result.Succeeded, message = result.Succeeded ? "Request rejected" : result.Error });
        }



        [HttpGet]
        public async Task<IActionResult> AvailableProjects()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var result = await _projectMembershipService.GetAvailableActiveProjectsForUserAsync(Guid.Parse(userId));

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Failed to load available projects.");
                return View(Array.Empty<Project>());
            }

            return View(result.Result);
        }

        [HttpGet]
        public async Task<IActionResult> MyRequests()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var result = await _projectMembershipService.GetPendingRequestsForUserAsync(Guid.Parse(userId));

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Failed to load your requests.");
                return View(Array.Empty<ProjectRequest>());
            }

            return View(result.Result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestJoin(Guid projectId, string message)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "User not authenticated" });

            var result = await _projectMembershipService.RequestProjectAssignmentAsync(
                projectId,
                Guid.Parse(userId),
                message);

            if (result.Succeeded)
            {

                return Json(new { success = true, message = "Request submitted successfully" });
            }

            return Json(new { success = false, message = result.Error });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRequest(Guid requestId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "User not authenticated" });

            var result = await _projectMembershipService.CancelProjectRequestAsync(requestId);

            return Json(new
            {
                success = result.Succeeded,
                message = result.Succeeded ? "Request canceled" : result.Error
            });
        }




    }
}

