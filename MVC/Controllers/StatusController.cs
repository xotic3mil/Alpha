using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using Business.Services;
using Business.Interfaces;

namespace MVC.Controllers
{
    public class StatusController(IStatusTypeService statusService) : Controller
    {

        private readonly IStatusTypeService _statusService = statusService;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new StatusViewModel();
            await PopulateViewModelAsync(viewModel);
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StatusViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewModelAsync(model);
                return View("Index", model);
            }

            var result = await _statusService.CreateStatus(model.Form);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "status created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to create status.");
            await PopulateViewModelAsync(model);
            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StatusViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewModelAsync(model);
                ViewBag.OpenEditModal = true;
                return View("Index", model);
            }

            var result = await _statusService.UpdateStatusAsync(model.Form);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "status updated successfully.";
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

            var result = await _statusService.DeleteStatusAsync(id);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ErrorMessage = result.Error ?? "Failed to delete service.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetStatusById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid service ID");
            }

            var result = await _statusService.GetStatusByIdAsync(id);

            if (!result.Succeeded || result.Result == null)
            {
                return NotFound("Service not found");
            }

            return Json(result.Result);
        }




        private async Task PopulateViewModelAsync(StatusViewModel model)
        {
            var statuses = await _statusService.GetStatusesAsync();

            if (statuses.Succeeded)
            {
                model.Statuses = statuses.Result;
            }
        }

    }
}
