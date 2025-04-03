using Business.Dtos;
using Business.Interfaces;
using Business.Models;
using Business.Services;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public ProjectController(
            IProjectsService projectService,
            IStatusTypeService statusService,
            IServicesService serviceService,
            ICustomersService customerService,
            IUserService userService)
        {
            _projectService = projectService;
            _statusService = statusService;
            _serviceService = serviceService;
            _customerService = customerService;
            _userService = userService;
        }



        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var project = await _projectService.GetProjectsAsync();
            var status = await _statusService.GetStatusesAsync();
            var service = await _serviceService.GetServicesAsync();
            var customer = await _customerService.GetCustomersAsync();

            ProjectViewModel viewModel = new ProjectViewModel()
            {
                Projects = project.Result,
                Statuses = status.Result,
                Services = service.Result,
                Customers = customer.Result

            };

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
            if (ModelState.IsValid)
            {
                var form = model.Form;
                var creationResult = await _projectService.CreateProjectAsync(form);

                if (creationResult.Succeeded)
                {
   
                    model.Form = new ProjectRegForm();
                    await PopulateViewModelAsync(model);

                    ViewBag.SuccessMessage = "Project created successfully.";
                    return View("Index", model);
                }

                ModelState.AddModelError(string.Empty, creationResult.Error);
            }

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



    }
}
