using Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using Business.Dtos;
using Business.Services;
using Domain.Models;

namespace MVC.Controllers
{
    [Authorize]
    public class MemberController(IUserService userService, ILogger<MemberController> logger) : Controller
    {
        private readonly IUserService _userService = userService;
        private readonly ILogger<MemberController> _logger = logger;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var viewModel = new MemberViewModel();
            await PopulateViewModelAsync(viewModel);
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MemberViewModel model, IFormFile? UserImage)
        {

            if (UserImage == null || UserImage.Length == 0)
            {
                ModelState.Remove("UserImage");
            }

            if (!ModelState.IsValid)
            {
                await PopulateViewModelAsync(model);
                return View("Index", model);
            }
            var form = model.Form;

            if (UserImage != null && UserImage.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(UserImage.FileName).ToLowerInvariant();

                if (allowedExtensions.Contains(extension))
                {
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await UserImage.CopyToAsync(fileStream);
                    }

                    form.AvatarUrl = $"/uploads/avatars/{fileName}";
                }
                else
                {
                    ModelState.AddModelError("UserImage", "Invalid file type. Allowed types: .jpg, .jpeg, .png, .gif");
                    await PopulateViewModelAsync(model);
                    return View("Index", model);
                }
            }
            else
            {
                form.AvatarUrl = "/images/member-template-1.svg";
            }

            var creationResult = await _userService.CreateUser(form);
            _logger.LogInformation("User creation result: {@Result}", new { Succeeded = creationResult.Succeeded, Error = creationResult.Error });

            if (creationResult.Succeeded)
            {
                model.Form = new UserRegForm();
                ViewBag.SuccessMessage = "Team member added successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, creationResult.Error ?? "Failed to create team member.");
            await PopulateViewModelAsync(model);
            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MemberViewModel model, IFormFile? UserImage)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewModelAsync(model);
                ViewBag.OpenEditModal = true;
                return View("Index", model);
            }

            if (UserImage != null && UserImage.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(UserImage.FileName).ToLowerInvariant();

                if (allowedExtensions.Contains(extension))
                {
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await UserImage.CopyToAsync(fileStream);
                    }

                    model.Form.AvatarUrl = $"/uploads/avatars/{fileName}";
                }
                else
                {
                    ModelState.AddModelError("UserImage", "Invalid file type. Allowed types: .jpg, .jpeg, .png, .gif");
                    await PopulateViewModelAsync(model);
                    ViewBag.OpenEditModal = true;
                    return View("Index", model);
                }
            }

            var user = new User
            {
                FirstName = model.Form.FirstName,
                LastName = model.Form.LastName,
                Email = model.Form.Email,
                PhoneNumber = model.Form.PhoneNumber,
                AvatarUrl = model.Form.AvatarUrl
            };

            var result = await _userService.UpdateUser(user);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Team member updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.Error ?? "Failed to update team member.");
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
                return BadRequest("User ID cannot be empty.");
            }

            var result = await _userService.DeleteUserAsync(id);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Team member deleted successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ErrorMessage = result.Error ?? "Failed to delete team member.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid user ID");
            }

            var users = await _userService.GetUsersAsync();
            if (!users.Succeeded)
            {
                return NotFound("Unable to retrieve users");
            }

            var user = users.Result.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            return Json(user);
        }




        private async Task PopulateViewModelAsync(MemberViewModel model)
        {
            var users = await _userService.GetUsersAsync();

            if (users.Succeeded)
            {
                model.Users = users.Result;
            }
        }



    }
}
