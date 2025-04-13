using Business.Dtos;
using Business.Interfaces;
using Business.Services;
using Data.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Diagnostics;

namespace MVC.Controllers;

[Authorize]
public class ProjectController(
    IProjectsService projectService,
    IStatusTypeService statusService,
    IServicesService serviceService,
    ICustomersService customerService,
    IUserService userService,
    UserManager<UserEntity> userManager,
    IProjectMembershipService projectMembershipService
        ) : Controller
{
    private readonly IProjectsService _projectService = projectService;
    private readonly IStatusTypeService _statusService = statusService;
    private readonly IServicesService _serviceService = serviceService;
    private readonly ICustomersService _customerService = customerService;
    private readonly IUserService _userService = userService;
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly IProjectMembershipService _projectMembershipService = projectMembershipService;

    [HttpGet]
    public async Task<IActionResult> Index(string statusFilter = null)
    {
        try
        {
            var viewModel = new ProjectViewModel();
            await PopulateViewModelAsync(viewModel);

            ViewBag.StatusFilter = statusFilter;
            var allProjects = viewModel.Projects.ToList();

            var statusCounts = allProjects
                .Where(p => p.Status?.StatusName != null)
                .GroupBy(p => p.Status!.StatusName)
                .ToDictionary(g => g.Key!, g => g.Count());

            ViewBag.StatusCounts = statusCounts;
            ViewBag.TotalCount = allProjects.Count;


            if (!string.IsNullOrEmpty(statusFilter))
            {
                viewModel.Projects = viewModel.Projects.Where(p => p.Status?.StatusName == statusFilter);
            }

            if (!User.IsInRole("Admin") && !User.IsInRole("ProjectManager"))
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(userId))
                {
                    var userGuid = Guid.Parse(userId);

                    var userProjectsResult = await _projectMembershipService.GetUserProjectsAsync(userGuid);
                    if (userProjectsResult.Succeeded)
                    {
                        viewModel.UserProjectIds = userProjectsResult.Result
                            .Select(p => p.Id)
                            .ToList();
                    }

                    var pendingRequestsResult = await _projectMembershipService.GetPendingRequestsForUserAsync(userGuid);
                    if (pendingRequestsResult.Succeeded)
                    {
                        viewModel.UserPendingRequestProjectIds = pendingRequestsResult.Result
                            .Select(r => r.ProjectId)
                            .ToList();
                    }
                }
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
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

    [HttpGet]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<IActionResult> Details(Guid? requestId)
    {
        if (requestId.HasValue)
        {

            var requestResult = await _projectMembershipService.GetRequestByIdAsync(requestId.Value);
            if (!requestResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Failed to load request details";
                return RedirectToAction("Index");
            }
            var projectResult = await _projectService.GetProjectWithDetailsAsync(requestResult.Result.ProjectId);
            if (!projectResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Failed to load project details";
                return RedirectToAction("Index");
            }

            var viewModel = new ProjectViewModel
            {
                Projects = new List<Project> { projectResult.Result },
                ProjectRequest = requestResult.Result
            };

            return View("Details", viewModel);
        }

        return RedirectToAction("Index");
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

    [HttpGet]
    [Route("Project/Details/{id}")]
    public async Task<IActionResult> ViewProjectDetails(Guid id)
    {
        var projectResult = await _projectService.GetProjectWithDetailsAsync(id);
        if (!projectResult.Succeeded)
        {
            TempData["ErrorMessage"] = "Project not found";
            return RedirectToAction("Index");
        }

        ViewBag.OpenProjectDetails = true;
        ViewBag.ProjectIdToOpen = id;

        return View("Index", new ProjectViewModel { Projects = new List<Project> { projectResult.Result } });
    }





}
