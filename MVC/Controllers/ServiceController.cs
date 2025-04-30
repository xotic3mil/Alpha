using Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;


namespace MVC.Controllers
{
    [Authorize]
    public class ServiceController : Controller
    {
        private readonly IServicesService _serviceService;

        public ServiceController(IServicesService serviceService)
        {
            _serviceService = serviceService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new ServiceViewModel();
            await PopulateViewModelAsync(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewModelAsync(model);
                return View("Index", model);
            }

            var result = await _serviceService.CreateServiceAsync(model.Form);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Service created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to create service.");
            await PopulateViewModelAsync(model);
            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewModelAsync(model);
                ViewBag.OpenEditModal = true;
                return View("Index", model);
            }

            var result = await _serviceService.UpdateServiceAsync(model.Form);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Service updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to update service.");
            await PopulateViewModelAsync(model);
            ViewBag.OpenEditModal = true;
            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Service ID cannot be empty.");
            }

            var result = await _serviceService.DeleteServiceAsync(id);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ErrorMessage = result.Error ?? "Failed to delete service.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetServiceById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid service ID");
            }

            var result = await _serviceService.GetServiceByIdAsync(id);

            if (!result.Succeeded || result.Result == null)
            {
                return NotFound("Service not found");
            }

            return Json(result.Result);
        }

        private async Task PopulateViewModelAsync(ServiceViewModel model)
        {
            var services = await _serviceService.GetServicesAsync();

            if (services.Succeeded)
            {
                model.Services = services.Result;
            }
        }
    }
}