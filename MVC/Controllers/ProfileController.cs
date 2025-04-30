using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Business.Interfaces;

namespace MVC.Controllers;

[Authorize]
public class ProfileController(
    UserManager<UserEntity> userManager,
    IUserService userService,
    IWebHostEnvironment hostEnvironment) : Controller
{
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly IUserService _userService = userService;
    private readonly IWebHostEnvironment _hostEnvironment = hostEnvironment;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UserEntity model, IFormFile? avatarImage)
    {
        System.Diagnostics.Debug.WriteLine($"Received update request for user: {model?.FirstName} {model?.LastName}");

        ModelState.Remove("AvatarUrl");


        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"ModelState errors: {string.Join(", ", errors)}");
            return Json(new { success = false, message = string.Join(", ", errors) });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new { success = false, message = "User not found." });
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PhoneNumber = model.PhoneNumber;


        if (avatarImage != null && avatarImage.Length > 0)
        {
            try
            {
                string avatarPath = await _userService.ProcessAvatarImageAsync(avatarImage, _hostEnvironment.WebRootPath);
                user.AvatarUrl = avatarPath;
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        System.Diagnostics.Debug.WriteLine($"Updating user with FirstName={user.FirstName}, LastName={user.LastName}");
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            System.Diagnostics.Debug.WriteLine("User update succeeded");
            return Json(new { success = true, message = "Your profile has been updated successfully!" });
        }

        var identityErrors = string.Join(", ", result.Errors.Select(e => e.Description));
        System.Diagnostics.Debug.WriteLine($"Update failed: {identityErrors}");
        return Json(new { success = false, message = identityErrors });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        if (string.IsNullOrEmpty(currentPassword))
        {
            ModelState.AddModelError("currentPassword", "Current password is required.");
        }

        if (string.IsNullOrEmpty(newPassword))
        {
            ModelState.AddModelError("newPassword", "New password is required.");
        }

        if (string.IsNullOrEmpty(confirmPassword))
        {
            ModelState.AddModelError("confirmPassword", "Confirm password is required.");
        }

        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError("confirmPassword", "The new password and confirmation password do not match.");
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, message = string.Join(", ", errors) });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new { success = false, message = "User not found." });
        }

        var changePasswordResult = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var errors = string.Join(", ", changePasswordResult.Errors.Select(e => e.Description));
            return Json(new { success = false, message = errors });
        }

        return Json(new { success = true, message = "Your password has been changed successfully." });
    }
}