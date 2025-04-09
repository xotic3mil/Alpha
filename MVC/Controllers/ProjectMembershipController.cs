using Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace MVC.Controllers
{
    [Authorize]
    public class ProjectMembershipController(IProjectMembershipService projectMembershipService) : Controller
    {
        private readonly IProjectMembershipService _projectMembershipService = projectMembershipService;

        [HttpGet]
        public async Task<IActionResult> GetAvailableUsers(Guid projectId)
        {
            var result = await _projectMembershipService.GetAvailableUsersForProjectAsync(projectId);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserToProject(Guid projectId, Guid userId)
        {
            var result = await _projectMembershipService.AddUserToProjectAsync(projectId, userId);
            return Json(new { success = result.Succeeded, message = result.Succeeded ? "User added successfully" : result.Error });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserFromProject(Guid projectId, Guid userId)
        {
            var result = await _projectMembershipService.RemoveUserFromProjectAsync(projectId, userId);
            return Json(new { success = result.Succeeded, message = result.Succeeded ? "User removed successfully" : result.Error });
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

            var result = await _projectMembershipService.GetProjectMembersAsync(projectId);

            if (!result.Succeeded)
                return Json(new { success = false, message = result.Error });

            return Json(new
            {
                success = true,
                members = result.Result.Select(m => new {
                    id = m.Id,
                    name = $"{m.FirstName} {m.LastName}",
                    email = m.Email,
                    avatarUrl = m.AvatarUrl ?? "/images/avatar-template-1.svg"
                }).ToList() // 
            });
        }
    }
}


