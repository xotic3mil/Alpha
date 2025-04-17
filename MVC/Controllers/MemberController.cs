using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using Data.Entities;
using Domain.Models;
using Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Business.Interfaces;
using Business.Dtos;

namespace MVC.Controllers
{
    [Authorize]
    public class MemberController(
        UserManager<UserEntity> userManager,
        IUserService userService,
        IWebHostEnvironment hostEnvironment,
        RoleManager<RoleEntity> roleManager) : Controller
    {
        private readonly UserManager<UserEntity> _userManager = userManager;
        private readonly IUserService _userService = userService;
        private readonly RoleManager<RoleEntity> _roleManager = roleManager;
        private readonly IWebHostEnvironment _hostEnvironment = hostEnvironment;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var viewModel = new MemberViewModel();
            await PopulateViewModelAsync(viewModel);

            viewModel.Roles = await _roleManager.Roles.ToListAsync();

            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid user ID");
            }

            var userEntity = await _userManager.FindByIdAsync(id.ToString());
            if (userEntity == null)
            {
                return NotFound("User not found");
            }

            var user = userEntity.MapTo<User>();

            return Json(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserRegForm model, string selectedRole, IFormFile UserImage)
        {
            ModelState.Remove("UserImage");
            model.TermsAndCondition = true;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

                Console.WriteLine($"Model validation failed with {errors.Count} error groups");
                return Json(new { success = false, errors = errors });
            }

            if (UserImage != null && UserImage.Length > 0)
            {
                try
                {
                    string avatarPath = await _userService.ProcessAvatarImageAsync(UserImage, _hostEnvironment.WebRootPath);
                    model.AvatarUrl = avatarPath;
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return Json(new { success = false, errors = new Dictionary<string, string[]> { { "UserImage", new[] { ex.Message } } } });
                }
            }
            else
            {
                model.AvatarUrl = "/images/member-template-1.svg";
            }
            if (!string.IsNullOrEmpty(selectedRole) && !await _roleManager.RoleExistsAsync(selectedRole))
            {
                return Json(new { success = false, errors = new Dictionary<string, string[]> { { "selectedRole", new[] { $"Role '{selectedRole}' does not exist." } } } });
            }

            var result = await _userService.CreateUserWithRoleAsync(model, selectedRole);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User created successfully with role: {selectedRole ?? "none"}";
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Member") });
            }

            ModelState.AddModelError("", result.Error);
            return Json(new { success = false, errors = new Dictionary<string, string[]> { { "", new[] { result.Error } } } });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid user ID");
            }

            var userEntity = await _userManager.FindByIdAsync(id.ToString());
            if (userEntity == null)
            {
                return NotFound("User not found");
            }

            var user = userEntity.MapTo<User>();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (Request.Form.Files.Count > 0)
            {
                var file = Request.Form.Files.FirstOrDefault();
                var webRootPath = _hostEnvironment.WebRootPath;
                try
                {
                    string avatarPath = await _userService.ProcessAvatarImageAsync(file, webRootPath);
                    model.AvatarUrl = avatarPath;
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }
            }

            var result = await _userService.UpdateUser(model);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result.Error);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid user ID");
            }

            var result = await _userService.DeleteUserAsync(id);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = result.Error;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToRole(string userId, string roleName)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
            {
                return BadRequest("User ID and role name are required");
            }

            var result = await _userService.AddUserToRole(userId, roleName);

            return Json(new { success = result.Succeeded, message = result.Succeeded ? "Role added successfully." : result.Error });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromRole(string userId, string roleName)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
            {
                return BadRequest("User ID and role name are required");
            }

            var result = await _userService.RemoveUserFromRole(userId, roleName);

            return Json(new { success = result.Succeeded, message = result.Succeeded ? "Role removed successfully." : result.Error });
        }

        private async Task PopulateViewModelAsync(MemberViewModel model)
        {
            try
            {
                var userEntities = await _userManager.Users.ToListAsync();

                var users = userEntities.Select(entity => {
                    var user = entity.MapTo<User>();
                    if (string.IsNullOrEmpty(user.AvatarUrl))
                    {
                        user.AvatarUrl = "/images/member-template-1.svg";
                    }
                    return user;
                }).ToList();

                model.Users = users;
            }
            catch (Exception ex)
            {
                model.Users = new List<User>();
            }
        }
    }
}