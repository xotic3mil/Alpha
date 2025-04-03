using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Business.Dtos;
using Data.Entities;
using Business.Factories;
using Business.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace MVC.Controllers;

public class AuthController(
    IUserService userService,
    UserManager<UserEntity> userManager,
    SignInManager<UserEntity> signInManager,
    ILogger<AuthController> logger
    ) : Controller
{
    private readonly IUserService _userService = userService;
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly SignInManager<UserEntity> _signInManager = signInManager;
    private readonly ILogger<AuthController> _logger = logger;

    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(UserLoginForm model)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Login failed due to invalid model state.");
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null)
        {
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in successfully.");
                return RedirectToAction("Index", "Project");
            }
            else
            {
                _logger.LogWarning("Invalid login attempt.");
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
        }
        else
        {
            _logger.LogWarning("Invalid login attempt.");
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Login", "Auth");
    }

    [Authorize]
    public IActionResult SecretPage()
    {
        return View();
    }


    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(UserRegForm model)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Registration failed due to invalid model state.");

            foreach (var state in ModelState.Values)
            {
                foreach (var error in state.Errors)
                {
                    _logger.LogWarning("Model Error: {ErrorMessage}", error.ErrorMessage);
                }
            }

            return View(model);
        }

        var userResult = await _userService.CreateUser(model);
        if (userResult.Succeeded)
        {
            _logger.LogInformation("Redirecting to Home after successful registration.");
            return RedirectToAction("Index", "Home");
        }

        _logger.LogError("Registration failed for Email: {Email}. Error: {Error}", model.Email, userResult.Error);
        ModelState.AddModelError(string.Empty, userResult.Error ?? "Registration failed. Please try again.");
        return View(model);
    }

    public IActionResult Admin()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminLogin(UserLoginForm model)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Admin login failed due to invalid model state.");
            return View("Admin", model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null)
        {
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin)
            {
                _logger.LogWarning("Unauthorized admin login attempt.");
                ModelState.AddModelError(string.Empty, "You are not authorized to access the admin panel.");
                return View("Admin", model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("Admin user logged in successfully.");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                _logger.LogWarning("Invalid admin login attempt.");
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
        }
        else
        {
            _logger.LogWarning("Admin login failed - user not found.");
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        return View("Admin", model);
    }
}
