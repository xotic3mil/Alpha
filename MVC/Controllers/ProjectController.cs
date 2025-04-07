using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Diagnostics;

namespace MVC.Controllers;

[Authorize]
public class ProjectController : Controller
{
    private readonly IProjectsService _projectService;
    private readonly IStatusTypeService _statusService;
    private readonly IServicesService _serviceService;
    private readonly ICustomersService _customerService;
    private readonly IUserService _userService;
    private readonly UserManager<UserEntity> _userManager;

    public ProjectController(
        IProjectsService projectService,
        IStatusTypeService statusService,
        IServicesService serviceService,
        ICustomersService customerService,
        IUserService userService,
        UserManager<UserEntity> userManager
        )
    {
        _projectService = projectService;
        _statusService = statusService;
        _serviceService = serviceService;
        _customerService = customerService;
        _userService = userService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string statusFilter = null)
    {
        try
        {
            var viewModel = new ProjectViewModel();
            await PopulateViewModelAsync(viewModel);

            var allProjects = viewModel.Projects?.ToList() ?? new List<Project>();

            var statusCounts = allProjects
                .GroupBy(p => p.Status?.StatusName ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.StatusFilter = statusFilter;
            ViewBag.TotalCount = allProjects.Count;
            ViewBag.StatusCounts = statusCounts;

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter.ToLower() != "all")
            {
                viewModel.Projects = allProjects
                    .Where(p => string.Equals(p.Status?.StatusName, statusFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Index action: {ex.Message}");

            ViewBag.StatusFilter = null;
            ViewBag.TotalCount = 0;
            ViewBag.StatusCounts = new Dictionary<string, int>();

            return View(new ProjectViewModel());
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Project ID cannot be empty.");
        }

        var result = await _projectService.DeleteProjectAsync(id);

        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction("Error", new { errorMessage = result.Error });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectViewModel model, IFormFile? ProjectImage)
    {
        if (ProjectImage == null || ProjectImage.Length == 0)
        {
            ModelState.Remove("ProjectImage");
        }

        if (!ModelState.IsValid)
        {
            await PopulateViewModelAsync(model);
            return View("Index", model);
        }

        var form = model.Form;

        if (ProjectImage != null && ProjectImage.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg" };
            var extension = Path.GetExtension(ProjectImage.FileName).ToLowerInvariant();

            if (allowedExtensions.Contains(extension))
            {
                var fileName = $"{Guid.NewGuid()}{extension}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "projects");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProjectImage.CopyToAsync(fileStream);
                }

                form.ImageUrl = $"/uploads/projects/{fileName}";
            }
            else
            {
                ModelState.AddModelError("ProjectImage", "Invalid file type. Allowed types: .jpg, .jpeg, .png, .gif, .svg");
                await PopulateViewModelAsync(model);
                return View("Index", model);
            }
        }
        else
        {
            form.ImageUrl = "/images/project-template-1.svg";
        }

        var creationResult = await _projectService.CreateProjectAsync(form);

        if (creationResult.Succeeded)
        {
            model.Form = new ProjectRegForm();
            ViewBag.SuccessMessage = "Project created successfully.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, creationResult.Error);
        await PopulateViewModelAsync(model);
        return View("Index", model);
    }

    private async Task PopulateViewModelAsync(ProjectViewModel model)
    {
        var project = await _projectService.GetProjectsAsync();
        var status = await _statusService.GetStatusesAsync();
        var service = await _serviceService.GetServicesAsync();
        var customer = await _customerService.GetCustomersAsync();

        model.Projects = project.Result;
        model.Statuses = status.Result;
        model.Services = service.Result;
        model.Customers = customer.Result;
    }

    public IActionResult Error(string errorMessage)
    {
        var errorViewModel = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            ErrorMessage = errorMessage
        };
        return View(errorViewModel);
    }

    [HttpGet]
    public async Task<IActionResult> GetProjectById(Guid id)
    {
        if (id == Guid.Empty)
        { return BadRequest("Invalid project ID"); }

        var result = await _projectService.GetProjectAsync(id);

        if (!result.Succeeded || result.Result == null)
        { return NotFound("Project not found"); }

        return Json(result.Result);
    }

    [HttpGet]
    public async Task<IActionResult> GetProjectByIdWithDetails(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Invalid project ID");
        }

        var result = await _projectService.GetProjectWithDetailsAsync(id);

        if (!result.Succeeded || result.Result == null)
        {
            return NotFound("Project not found");
        }

        return Json(result.Result);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProjectViewModel model, IFormFile? ProjectImage)
    {

        if (ProjectImage == null || ProjectImage.Length == 0)
        {
            ModelState.Remove("ProjectImage");
        }

        if (!ModelState.IsValid)
        {
            await PopulateViewModelAsync(model); 
            ViewBag.OpenEditModal = true;
            return View("Index", model);
        }

        var form = model.Form;
        if (ProjectImage == null || ProjectImage.Length == 0)
        {
            var existingProject = await _projectService.GetProjectAsync(form.Id);
            if (existingProject.Succeeded && existingProject.Result != null)
            {
                form.ImageUrl = existingProject.Result.ImageUrl;
            }
        }
        else
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg" };
            var extension = Path.GetExtension(ProjectImage.FileName).ToLowerInvariant();

            if (allowedExtensions.Contains(extension))
            {
                var fileName = $"{Guid.NewGuid()}{extension}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "projects");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProjectImage.CopyToAsync(fileStream);
                }
                if (form.ImageUrl != null && !form.ImageUrl.Contains("project-template-1.svg"))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                        form.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Failed to delete old project image: {ex.Message}");
                        }
                    }
                }

                form.ImageUrl = $"/uploads/projects/{fileName}";
            }
            else
            {
                ModelState.AddModelError("ProjectImage", "Invalid file type. Allowed types: .jpg, .jpeg, .png, .gif, .svg");
                await PopulateViewModelAsync(model);
                ViewBag.OpenEditModal = true;
                return View("Index", model);
            }
        }

        var updateResult = await _projectService.UpdateProjectAsync(form);

        if (updateResult.Succeeded)
        {
            ViewBag.SuccessMessage = "Project updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError(string.Empty, updateResult.Error ?? "Failed to update project");
        await PopulateViewModelAsync(model);
        ViewBag.OpenEditModal = true;
        return View("Index", model);
    }

}
