using Microsoft.AspNetCore.Mvc;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Business.Interfaces;

namespace MVC.ViewComponents
{
    public class RequestsNavViewComponent(
        IProjectMembershipService projectMembershipService,
        UserManager<UserEntity> userManager) : ViewComponent
    {
        private readonly IProjectMembershipService _projectMembershipService = projectMembershipService;
        private readonly UserManager<UserEntity> _userManager = userManager;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int pendingRequestsCount = 0;

            if (User.Identity.IsAuthenticated)
            {
                string userId = _userManager.GetUserId(HttpContext.User);
                if (!string.IsNullOrEmpty(userId))
                {
                    var result = await _projectMembershipService.GetPendingRequestsForUserAsync(Guid.Parse(userId));
                    if (result.Succeeded)
                    {
                        pendingRequestsCount = result.Result.Count();
                    }
                }
            }

            ViewBag.PendingRequestsCount = pendingRequestsCount;
            return View();
        }
    }
}