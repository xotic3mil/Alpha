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
using System.Diagnostics;

namespace MVC.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly IUserService _userService;
        private readonly RoleManager<RoleEntity> _roleManager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public MemberController(
            UserManager<UserEntity> userManager,
            IUserService userService,
            IWebHostEnvironment hostEnvironment,
            RoleManager<RoleEntity> roleManager)
        {
            _userManager = userManager;
            _userService = userService;
            _roleManager = roleManager;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            try
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
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid user ID");
            }

            var userEntity = await _userManager.Users
                .Include(u => u.Projects)
                    .ThenInclude(p => p.Status)
                .Include(u => u.Projects)
                    .ThenInclude(p => p.Customer)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (userEntity == null)
            {
                return NotFound("User not found");
            }

            var user = userEntity.MapTo<User>();

            var roles = await _userManager.GetRolesAsync(userEntity);
            user.RoleName = roles.FirstOrDefault();

            if (userEntity.Projects != null && userEntity.Projects.Any())
            {
                var activeProjects = userEntity.Projects
                    .Where(p => p.Status == null ||
                          !p.Status.StatusName.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                    .Select(p => new {
                        id = p.Id,
                        name = p.Name,
                        description = p.Description,
                        statusId = p.StatusId,
                        status = p.Status != null ? new
                        {
                            name = p.Status.StatusName,
                            colorCode = p.Status.ColorCode
                        } : null
                    })
                    .ToList();

                return Json(new
                {
                    id = user.Id,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber,
                    title = user.Title,
                    avatarUrl = user.AvatarUrl,
                    roleName = user.RoleName,
                    projects = activeProjects
                });
            }
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

                return Json(new { success = false, errors = errors });
            }

            model.AvatarUrl = "/images/member-template-1.svg";
            if (UserImage != null && UserImage.Length > 0)
            {
                try
                {
                    string avatarPath = await _userService.ProcessAvatarImageAsync(UserImage, _hostEnvironment.WebRootPath);
                    model.AvatarUrl = avatarPath;
                }
                catch (ArgumentException ex)
                {
                    return Json(new { success = false, errors = new Dictionary<string, string[]> { { "UserImage", new[] { ex.Message } } } });
                }
            }

            if (!string.IsNullOrEmpty(selectedRole) && !await _roleManager.RoleExistsAsync(selectedRole))
            {
                return Json(new { success = false, errors = new Dictionary<string, string[]> { { "selectedRole", new[] { $"Role '{selectedRole}' does not exist." } } } });
            }

            var result = await _userService.CreateUserWithRoleAsync(model, selectedRole);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Team member created successfully with role: {selectedRole ?? "none"}";
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Member") });
            }

            return Json(new { success = false, errors = new Dictionary<string, string[]> { { "", new[] { result.Error } } } });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserRegForm Form, Guid userId, string selectedRole, IFormFile UserImage)
        {
            ModelState.Remove("UserImage");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

                return Json(new { success = false, errors = errors });
            }
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Json(new { success = false, errors = new Dictionary<string, string[]> { { "", new[] { "User not found." } } } });
            }

            user.FirstName = Form.FirstName;
            user.LastName = Form.LastName;
            user.Email = Form.Email;
            user.NormalizedEmail = Form.Email.ToUpper();
            user.UserName = Form.Email;
            user.NormalizedUserName = Form.Email.ToUpper();
            user.PhoneNumber = Form.PhoneNumber;
            user.Title = Form.Title;

            if (UserImage != null && UserImage.Length > 0)
            {
                try
                {
                    string avatarPath = await _userService.ProcessAvatarImageAsync(UserImage, _hostEnvironment.WebRootPath);
                    user.AvatarUrl = avatarPath;
                }
                catch (ArgumentException ex)
                {
                    return Json(new { success = false, errors = new Dictionary<string, string[]> { { "UserImage", new[] { ex.Message } } } });
                }
            }
            else if (!string.IsNullOrEmpty(Form.AvatarUrl))
            {
                user.AvatarUrl = Form.AvatarUrl;
            }

            if (!string.IsNullOrEmpty(Form.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, Form.Password);

                if (!passwordResult.Succeeded)
                {
                    var errors = new Dictionary<string, string[]>();
                    errors["Password"] = passwordResult.Errors.Select(e => e.Description).ToArray();
                    return Json(new { success = false, errors = errors });
                }
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded && !string.IsNullOrEmpty(selectedRole))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);

                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }
                await _userManager.AddToRoleAsync(user, selectedRole);
            }

            if (result.Succeeded)
            {
                return Json(new { success = true, message = "Team member updated successfully.", redirectUrl = Url.Action("Index", "Member") });
            }

            var errorMessages = result.Errors.ToDictionary(
                e => string.Empty,
                e => new[] { e.Description }
            );
            return Json(new { success = false, errors = errorMessages });
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
                TempData["SuccessMessage"] = "Team member deleted successfully.";
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

        public IActionResult Error(string errorMessage)
        {
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = errorMessage
            };
            return View(errorViewModel);
        }

        private async Task PopulateViewModelAsync(MemberViewModel model)
        {
            try
            {
                var userEntities = await _userManager.Users.ToListAsync();

                var users = new List<User>();
                foreach (var entity in userEntities)
                {
                    var user = entity.MapTo<User>();
                    if (string.IsNullOrEmpty(user.AvatarUrl))
                    {
                        user.AvatarUrl = "/images/avatar-template-1.svg";
                    }

                    var roles = await _userManager.GetRolesAsync(entity);
                    user.RoleName = roles.FirstOrDefault();

                    users.Add(user);
                }

                model.Users = users;
                model.Roles = await _roleManager.Roles.ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error populating MemberViewModel: {ex.Message}");
                model.Users = new List<User>();
                model.Roles = new List<RoleEntity>();
            }
        }
    }
}