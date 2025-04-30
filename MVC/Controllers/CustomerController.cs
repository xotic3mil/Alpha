using Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;


namespace MVC.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ICustomersService _customerService;

        public CustomerController(ICustomersService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new CustomerViewModel();
            await PopulateViewModelAsync(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewModelAsync(model);
                return View("Index", model);
            }

            var result = await _customerService.CreateCustomerAsync(model.Form);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Customer created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to create customer.");
            await PopulateViewModelAsync(model);
            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewModelAsync(model);
                ViewBag.OpenEditModal = true;
                return View("Index", model);
            }

            var result = await _customerService.UpdateCustomerAsync(model.Form);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Customer updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to update customer.");
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
                return BadRequest("Customer ID cannot be empty.");
            }

            var result = await _customerService.DeleteCustomerAsync(id);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Customer deleted successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ErrorMessage = result.Error ?? "Failed to delete customer.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid customer ID");
            }

            var result = await _customerService.GetCustomerByIdAsync(id);

            if (!result.Succeeded || result.Result == null)
            {
                return NotFound("Customer not found");
            }

            return Json(result.Result);
        }

        private async Task PopulateViewModelAsync(CustomerViewModel model)
        {
            var customers = await _customerService.GetCustomersAsync();

            if (customers.Succeeded)
            {
                model.Customers = customers.Result;
            }
        }
    }
}