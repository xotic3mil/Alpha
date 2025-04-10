using Business.Interfaces;
using Data.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers;

[Authorize(Roles = "Admin,ProjectManager")]
public class AdminRequestsController(
    IProjectMembershipService projectMembershipService,
    UserManager<UserEntity> userManager,
    INotificationService notificationService) : Controller
{
    private readonly IProjectMembershipService _projectMembershipService = projectMembershipService;
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly INotificationService _notificationService = notificationService;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _projectMembershipService.GetAllPendingRequestsAsync();

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to load project requests");
            return View(Array.Empty<ProjectRequest>());
        }

        return View(result.Result.OrderByDescending(r => r.RequestDate).ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveRequest(Guid requestId)
    {
        var result = await _projectMembershipService.ApproveProjectRequestAsync(requestId);

        if (result.Succeeded)
        {
            var request = await _projectMembershipService.GetRequestByIdAsync(requestId);
            if (request.Succeeded)
            {
                await _notificationService.CreateUserNotificationAsync(
                    request.Result.UserId,
                    "Request Approved",
                    $"Your request to join project \"{request.Result.Project?.Name}\" has been approved.",
                    "ProjectRequestApproved");
            }
        }

        TempData["StatusMessage"] = result.Succeeded
            ? "Request approved successfully."
            : $"Error: {result.Error}";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectRequest(Guid requestId)
    {
        var result = await _projectMembershipService.RejectProjectRequestAsync(requestId);

        if (result.Succeeded)
        {
            var request = await _projectMembershipService.GetRequestByIdAsync(requestId);
            if (request.Succeeded)
            {
                await _notificationService.CreateUserNotificationAsync(
                    request.Result.UserId,
                    "Request Rejected",
                    $"Your request to join project \"{request.Result.Project?.Name}\" has been rejected.",
                    "ProjectRequestRejected");
            }
        }

        TempData["StatusMessage"] = result.Succeeded
            ? "Request rejected successfully."
            : $"Error: {result.Error}";

        return RedirectToAction(nameof(Index));
    }
}






