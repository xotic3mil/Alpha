using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Domain.Extensions;
using Domain.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace MVC.Controllers;

[Authorize]
public class ProjectTaskController(
    IProjectTaskService taskService,
    IProjectsService projectService,
    UserManager<UserEntity> userManager,
    IUserService userService) : Controller
{
    private readonly IProjectTaskService _taskService = taskService;
    private readonly IProjectsService _projectService = projectService;
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly IUserService _userService = userService;

    public async Task<IActionResult> Index(Guid? projectId = null)
    {
        ViewBag.ProjectId = projectId;

        if (projectId.HasValue)
        {
            var projectResult = await _projectService.GetProjectAsync(projectId.Value);
            if (!projectResult.Succeeded)
            {
                TempData["ErrorMessage"] = $"Project not found: {projectResult.Error}";
                return RedirectToAction("Index", "Project");
            }

            ViewBag.Project = projectResult.Result;
            var tasksResult = await _taskService.GetTasksByProjectAsync(projectId.Value);
            return View(tasksResult.Result);
        }
        else
        {
            var tasksResult = await _taskService.GetAllTasksAsync();
            return View(tasksResult.Result);
        }
    }

    public async Task<IActionResult> MyTasks()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return RedirectToAction("Login", "Auth");
        }

        var tasksResult = await _taskService.GetTasksAssignedToUserAsync(userId);
        return View(tasksResult.Result);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var taskResult = await _taskService.GetTaskByIdAsync(id);
        if (!taskResult.Succeeded)
        {
            TempData["ErrorMessage"] = "Task not found";
            return RedirectToAction(nameof(Index));
        }

        return View(taskResult.Result);
    }

    public async Task<IActionResult> Create(Guid projectId)
    {
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            var projectResult = await _projectService.GetProjectAsync(projectId);
            if (!projectResult.Succeeded)
            {
                return Json(new { succeeded = false, error = "Project not found" });
            }

            var users = await _userManager.Users.ToListAsync();
            var usersList = users.Select(u => new { id = u.Id.ToString(), name = u.UserName }).ToList();

            return Json(new
            {
                succeeded = true,
                projectName = projectResult.Result.Name,
                users = usersList
            });
        }

        var projectResult2 = await _projectService.GetProjectAsync(projectId);
        if (!projectResult2.Succeeded)
        {
            TempData["ErrorMessage"] = "Project not found";
            return RedirectToAction("Index", "Project");
        }

        var users2 = await _userManager.Users.ToListAsync();
        ViewBag.Users = new SelectList(users2, "Id", "Name");

        ViewBag.ProjectId = projectId;
        ViewBag.ProjectName = projectResult2.Result.Name;

        return View(new ProjectTask { ProjectId = projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectTask task)
    {
        Debug.WriteLine($"Creating task: {task?.Title}, ProjectId: {task?.ProjectId}");
        if (task == null)
        {
            return Json(new { succeeded = false, error = "Task data cannot be null" });
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new {
                    Field = x.Key,
                    Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList()
                })
                .ToList();

            Debug.WriteLine($"ModelState errors: {string.Join(", ", errors.SelectMany(e => $"{e.Field}: {string.Join(", ", e.Errors)}"))}");

            return Json(new { succeeded = false, error = "Invalid form data", details = errors });
        }

        var currentUserId = GetCurrentUserId();

        var user = await _userManager.FindByIdAsync(currentUserId.ToString());
        if (user == null)
        {
            Debug.WriteLine($"ERROR: User with ID {currentUserId} not found in database");
            return Json(new { succeeded = false, error = "Current user not found in the system" });
        }

        Debug.WriteLine($"Found user: {user.UserName} with ID: {user.Id}");


        task.CreatedById = GetCurrentUserId();
        task.CreatedAt = DateTime.UtcNow;

        Debug.WriteLine($"Creating task with CreatedById: {task.CreatedById}");

        var result = await _taskService.CreateTaskAsync(task);
        if (!result.Succeeded)
        {
            return Json(new { succeeded = false, error = result.Error });
        }

        return Json(new { succeeded = true, result = result.Result });
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        Debug.WriteLine($"Current user ID from claims: '{userId}'");

        if (string.IsNullOrEmpty(userId))
        {
            Debug.WriteLine("WARNING: User ID claim is null or empty!");
            return Guid.Empty;
        }

        try
        {
            var parsedId = Guid.Parse(userId);
            Debug.WriteLine($"Parsed user ID: {parsedId}");
            return parsedId;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR parsing user ID: {ex.Message}");
            return Guid.Empty;
        }
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var taskResult = await _taskService.GetTaskByIdAsync(id);
        if (!taskResult.Succeeded)
        {
            TempData["ErrorMessage"] = "Task not found";
            return RedirectToAction(nameof(Index));
        }

        var users = await _userManager.Users.ToListAsync();
        ViewBag.Users = new SelectList(users, "Id", "Name", taskResult.Result.AssignedToId);

        var projectResult = await _projectService.GetProjectAsync(taskResult.Result.ProjectId);
        if (projectResult.Succeeded)
        {
            ViewBag.ProjectName = projectResult.Result.Name;
        }

        return View(taskResult.Result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProjectTask task)
    {
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new {
                        Field = x.Key,
                        Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    })
                    .ToList();

                return Json(new { succeeded = false, error = "Invalid form data", details = errors });
            }

            var result = await _taskService.UpdateTaskAsync(task);
            return Json(new { succeeded = result.Succeeded, error = result.Error });
        }

        if (!ModelState.IsValid)
        {
            var users = await _userManager.Users.ToListAsync();
            ViewBag.Users = new SelectList(users, "Id", "Name", task.AssignedToId);
            return View(task);
        }

        var updateResult = await _taskService.UpdateTaskAsync(task);
        if (!updateResult.Succeeded)
        {
            TempData["ErrorMessage"] = $"Error updating task: {updateResult.Error}";
            return RedirectToAction(nameof(Edit), new { id = task.Id });
        }

        TempData["SuccessMessage"] = "Task updated successfully";
        return RedirectToAction(nameof(Index), new { projectId = task.ProjectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid id)
    {
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            var result = await _taskService.CompleteTaskAsync(id);
            return Json(new { succeeded = result.Succeeded, error = result.Error });
        }

        var completeResult = await _taskService.CompleteTaskAsync(id);
        if (!completeResult.Succeeded)
        {
            TempData["ErrorMessage"] = $"Error completing task: {completeResult.Error}";
        }
        else
        {
            TempData["SuccessMessage"] = "Task marked as completed";
        }

        var taskResult = await _taskService.GetTaskByIdAsync(id);
        if (taskResult.Succeeded)
        {
            return RedirectToAction("Index", "Project");
        }

        return RedirectToAction("Index", "Project");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            var result = await _taskService.DeleteTaskAsync(id);
            return Json(new { succeeded = result.Succeeded, error = result.Error });
        }

        var taskResult = await _taskService.GetTaskByIdAsync(id);
        if (!taskResult.Succeeded)
        {
            TempData["ErrorMessage"] = "Task not found";
            return RedirectToAction(nameof(Index));
        }

        var projectId = taskResult.Result.ProjectId;

        var deleteResult = await _taskService.DeleteTaskAsync(id);
        if (!deleteResult.Succeeded)
        {
            TempData["ErrorMessage"] = $"Error deleting task: {deleteResult.Error}";
        }
        else
        {
            TempData["SuccessMessage"] = "Task deleted successfully";
        }

        return RedirectToAction(nameof(Index), new { projectId });
    }

    [HttpGet]
    public async Task<IActionResult> GetTasksByProject(Guid projectId)
    {
        var result = await _taskService.GetTasksByProjectAsync(projectId);
        if (!result.Succeeded)
        {
            return Json(new { success = false, message = result.Error });
        }

        return Json(new { success = true, tasks = result.Result });
    }

    [HttpGet]
    public async Task<IActionResult> GetTaskById(Guid id)
    {
        var result = await _taskService.GetTaskByIdAsync(id);

        if (!result.Succeeded)
        {
            return Json(new { succeeded = false, error = result.Error ?? "Task not found" });
        }

        return Json(new { succeeded = true, result = result.Result });
    }

    [HttpGet]
    public async Task<IActionResult> GetTaskSummary(Guid projectId)
    {
        var result = await _taskService.GetTaskSummaryByProjectAsync(projectId);
        if (!result.Succeeded)
        {
            return Json(new { success = false, message = result.Error });
        }

        return Json(new
        {
            success = true,
            totalTasks = result.Result.TotalTasks,
            completedTasks = result.Result.CompletedTasks,
            completionPercentage = result.Result.CompletionPercentage
        });
    }
}