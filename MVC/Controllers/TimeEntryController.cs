using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Domain.Models;
using System.Security.Claims;
using System.Diagnostics;

namespace MVC.Controllers;

[Authorize]
public class TimeEntryController(
    ITimeEntryService timeEntryService,
    IProjectsService projectService,
    IProjectTaskService taskService,
    UserManager<UserEntity> userManager) : Controller
{
    private readonly ITimeEntryService _timeEntryService = timeEntryService;
    private readonly IProjectsService _projectService = projectService;
    private readonly IProjectTaskService _taskService = taskService;
    private readonly UserManager<UserEntity> _userManager = userManager;

    [HttpGet]
    public async Task<IActionResult> GetTimeEntriesByProject(Guid projectId)
    {
        var result = await _timeEntryService.GetTimeEntriesByProjectAsync(projectId);
        if (!result.Succeeded)
        {
            return Json(new { success = false, message = result.Error });
        }

        var entries = result.Result.Select(e => new {
            id = e.Id,
            date = e.Date.ToString("yyyy-MM-dd"),
            hours = e.Hours,
            description = e.Description,
            userName = e.User?.Email ?? "Unknown",
            taskTitle = e.Task?.Title ?? "No Task",
            isBillable = e.IsBillable,
            hourlyRate = e.HourlyRate,
            billableAmount = e.IsBillable ? e.Hours * (double)e.HourlyRate : 0
        }).ToList();

        return Json(new { success = true, timeEntries = entries });
    }

    [HttpGet]
    public async Task<IActionResult> GetTimeEntryById(Guid id)
    {
        var result = await _timeEntryService.GetTimeEntryByIdAsync(id);
        if (!result.Succeeded)
        {
            return Json(new { succeeded = false, error = result.Error });
        }

        return Json(new { succeeded = true, result = result.Result });
    }

    [HttpGet]
    public async Task<IActionResult> GetTimeEntrySummary(Guid projectId)
    {
        var result = await _timeEntryService.GetTimeEntrySummaryByProjectAsync(projectId);
        if (!result.Succeeded)
        {
            return Json(new { success = false, message = result.Error });
        }

        return Json(new
        {
            success = true,
            totalHours = result.Result.TotalHours,
            billableHours = result.Result.BillableHours,
            nonBillableHours = result.Result.NonBillableHours,
            totalBillableAmount = result.Result.TotalBillableAmount
        });
    }


    [HttpGet]
    public async Task<IActionResult> GetTasksByProject(Guid projectId)
    {
        var result = await _taskService.GetTasksByProjectAsync(projectId);
        if (!result.Succeeded)
        {
            return Json(new { success = false, message = result.Error });
        }

        var tasks = result.Result?.Select(t => new { id = t.Id, title = t.Title }).ToList();
        return Json(new { success = true, tasks });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAjax([FromForm] TimeEntry timeEntry)
    {
        if (!ModelState.IsValid)
        {
            return Json(new
            {
                succeeded = false,
                error = "Invalid form data",
                details = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }


        timeEntry.UserId = GetCurrentUserId();

        if (timeEntry.ProjectId == Guid.Empty)
        {
            return Json(new
            {
                succeeded = false,
                error = "Invalid project ID"
            });
        }

        timeEntry.Date = DateTime.SpecifyKind(timeEntry.Date, DateTimeKind.Utc);
        timeEntry.CreatedAt = DateTime.UtcNow;

        Debug.WriteLine($"Creating time entry: ProjectId={timeEntry.ProjectId}, UserId={timeEntry.UserId}, TaskId={timeEntry.TaskId}, Hours={timeEntry.Hours}, Date={timeEntry.Date}");

        var result = await _timeEntryService.CreateTimeEntryAsync(timeEntry);
        if (!result.Succeeded)
        {
            return Json(new { succeeded = false, error = result.Error });
        }

        return Json(new { succeeded = true, result = result.Result });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAjax([FromForm] TimeEntry timeEntry)
    {
        if (!ModelState.IsValid)
        {
            return Json(new
            {
                succeeded = false,
                error = "Invalid form data",
                details = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }

        Debug.WriteLine($"Updating time entry: ID={timeEntry.Id}, ProjectId={timeEntry.ProjectId}, Hours={timeEntry.Hours}");

        if (timeEntry.UserId == Guid.Empty)
        {
            timeEntry.UserId = GetCurrentUserId();
        }
        timeEntry.Date = DateTime.SpecifyKind(timeEntry.Date, DateTimeKind.Utc);

        var result = await _timeEntryService.UpdateTimeEntryAsync(timeEntry);
        if (!result.Succeeded)
        {
            return Json(new { succeeded = false, error = result.Error });
        }

        return Json(new { succeeded = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(Guid id)
    {
        var result = await _timeEntryService.DeleteTimeEntryAsync(id);
        if (!result.Succeeded)
        {
            return Json(new { succeeded = false, error = result.Error });
        }

        return Json(new { succeeded = true });
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            Debug.WriteLine("WARNING: User ID claim is null or empty!");
            return Guid.Empty;
        }

        try
        {
            return Guid.Parse(userId);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR parsing user ID: {ex.Message}");
            return Guid.Empty;
        }
    }
}