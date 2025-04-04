using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Diagnostics;

namespace MVC.Controllers
{
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

            var viewModel = new ProjectViewModel();
            await PopulateViewModelAsync(viewModel);


            var allProjects = viewModel.Projects.ToList();

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
        public async Task<IActionResult> Create(ProjectViewModel model)
        {

            await PopulateViewModelAsync(model);

            if (ModelState.IsValid)
            {
                var form = model.Form;
                var creationResult = await _projectService.CreateProjectAsync(form);

                if (creationResult.Succeeded)
                {
                    model.Form = new ProjectRegForm();
                    ViewBag.SuccessMessage = "Project created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, creationResult.Error);
            }

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
            {
                return BadRequest("Invalid project ID");
            }

            var result = await _projectService.GetProjectAsync(id);

            if (!result.Succeeded || result.Result == null)
            {
                return NotFound("Project not found");
            }

            return Json(result.Result);
        }


    }
}
