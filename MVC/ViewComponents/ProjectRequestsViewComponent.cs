using Business.Interfaces;
using Domain.Models;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MVC.ViewComponents
{
    public class ProjectRequestsViewComponent(
        IProjectMembershipService projectMembershipService,
        UserManager<UserEntity> userManager) : ViewComponent
    {
        private readonly IProjectMembershipService _projectMembershipService = projectMembershipService;
        private readonly UserManager<UserEntity> _userManager = userManager;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View(new List<ProjectRequest>());
            }

            string userId = _userManager.GetUserId(HttpContext.User);
            if (string.IsNullOrEmpty(userId))
            {
                return View(new List<ProjectRequest>());
            }

            var result = await _projectMembershipService.GetPendingRequestsForUserAsync(Guid.Parse(userId));

            return View(result.Succeeded ? result.Result : new List<ProjectRequest>());
        }
    }
}