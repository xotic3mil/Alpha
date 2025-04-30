using Business.Interfaces;
using Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace MVC.Controllers
{
    [Authorize]
    public class ProjectMembershipController(IProjectMembershipService projectMembershipService, IUserService userService) : Controller
    {
        private readonly IProjectMembershipService _projectMembershipService = projectMembershipService;
        private readonly IUserService _userService = userService;

        [HttpGet]
        public async Task<IActionResult> GetAvailableUsers(Guid? projectId = null)
        {
            try
            {
                if (projectId.HasValue && projectId.Value != Guid.Empty)
                {
                    var result = await _projectMembershipService.GetAvailableUsersForProjectAsync(projectId.Value);
                    if (!result.Succeeded)
                        return Json(new { success = false, message = result.Error });

                    return Json(new
                    {
                        success = true,
                        users = result.Result.Select(u => new
                        {
                            id = u.Id,
                            name = $"{u.FirstName} {u.LastName}",
                            email = u.Email,
                            avatarUrl = u.AvatarUrl ?? "/images/avatar-template-1.svg"
                        })
                    });
                }
                else
                {
                    var userResult = await _userService.GetUsersAsync();

                    if (!userResult.Succeeded)
                        return Json(new { success = false, message = userResult.Error });

                    return Json(new
                    {
                        success = true,
                        users = userResult.Result.Select(u => new
                        {
                            id = u.Id,
                            name = $"{u.FirstName} {u.LastName}",
                            email = u.Email,
                            avatarUrl = u.AvatarUrl ?? "/images/avatar-template-1.svg"
                        })
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUserToProject(Guid projectId, Guid userId)
        {
            var result = await _projectMembershipService.AddUserToProjectAsync(projectId, userId);
            return Json(new { success = result.Succeeded, message = result.Succeeded ? "User added successfully" : result.Error });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserFromProject(Guid projectId, Guid userId)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("ProjectManager"))
            {
                return Json(new { success = false, error = "You do not have permission to remove team members" });
            }

            var result = await _projectMembershipService.RemoveUserFromProjectAsync(projectId, userId);

            if (!result.Succeeded)
            {
                return Json(new { success = false, error = result.Error });
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingRequests(Guid projectId)
        {
            var result = await _projectMembershipService.GetPendingRequestsForProjectAsync(projectId);
            if (!result.Succeeded)
                return Json(new { success = false, message = result.Error });

            return Json(new
            {
                success = true,
                requests = result.Result.Select(r => new
                {
                    id = r.Id,
                    userName = $"{r.UserName}",
                    userId = r.UserId,
                    message = r.Message,
                    requestDate = r.RequestDate.ToString("MMM d, yyyy")
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectMembers(Guid projectId)
        {
            if (projectId == Guid.Empty)
            {
                return Json(new { success = false, message = "Invalid project ID" });
            }

            var result = await _projectMembershipService.GetProjectMembersAsync(projectId);
            if (!result.Succeeded)
            {
                return Json(new { success = false, message = result.Error });
            }

            bool canManageMembers = User.IsInRole("Admin") || User.IsInRole("ProjectManager");

            return Json(new
            {
                success = true,
                canManageMembers = canManageMembers,
                members = result.Result.Select(m => new
                {
                    id = m.Id,
                    name = $"{m.FirstName} {m.LastName}",
                    email = m.Email,
                    avatarUrl = m.AvatarUrl ?? "/images/avatar-template-1.svg"
                })
            });
        }

    }
}


