using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Business.Dtos;
using Data.Entities;
using Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Business.Services;
using System.Security.Claims;

namespace MVC.Controllers;

public class AuthController(
    IUserService userService,
    UserManager<UserEntity> userManager,
    SignInManager<UserEntity> signInManager,
    DatabaseInitializationService dbInitService,
    ILogger<AuthController> logger
    ) : Controller
{
    private readonly IUserService _userService = userService;
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly DatabaseInitializationService _dbInitService = dbInitService;
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
        var isInitialized = await _dbInitService.IsDatabaseInitializedAsync();

        if (!isInitialized)
        {
            ViewBag.FirstUser = true;
            ViewBag.Title = "Create Admin Account";
            return RedirectToAction("Register");
        }

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
    public async Task<IActionResult> Register()
    {
        bool isFirstUser = !await _dbInitService.IsDatabaseInitializedAsync();

        ViewBag.FirstUser = isFirstUser;

        if (isFirstUser)
        {
            _logger.LogInformation("Showing admin registration page for first-time setup");
        }

        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(UserRegForm model)
    {
        var isFirstUser = !await _dbInitService.IsDatabaseInitializedAsync();

        if (isFirstUser)
        {
            ViewBag.FirstUser = true;
            ViewBag.Title = "Create Admin Account";
            model.TermsAndCondition = true;
            ModelState.Remove(nameof(model.TermsAndCondition));
        }

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
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                if (isFirstUser)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                    _logger.LogInformation("First user created with Admin role: {Email}", model.Email);
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    _logger.LogInformation("Regular user created with User role: {Email}", model.Email);
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("User {Email} automatically logged in after registration", model.Email);
            }

            _logger.LogInformation("Redirecting to Home after successful registration.");
            return RedirectToAction("Index", "Project");
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
                return RedirectToAction("Index", "Project");
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

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetCurrentUserRoles()
    {
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);

        return Json(new
        {
            userId = user.Id,
            userName = user.UserName,
            roles = roles,
            isInAdminRole = User.IsInRole("Admin"),
            isInPMRole = User.IsInRole("ProjectManager")
        });
    }


    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string returnUrl = null)
    {
        var redirectUrl = Url.Action("ExternalLoginCallback", "Auth",
                            new { ReturnUrl = returnUrl });

        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
    {
        returnUrl ??= Url.Content("~/");

        if (remoteError != null)
        {
            _logger.LogError($"Error from external provider: {remoteError}");
            TempData["ErrorMessage"] = $"Error from external provider: {remoteError}";
            return RedirectToAction("Login");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogWarning("Error loading external login information.");
            TempData["ErrorMessage"] = "Error loading external login information.";
            return RedirectToAction("Login");
        }
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,
            isPersistent: false, bypassTwoFactor: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in with {LoginProvider}", info.LoginProvider);
            return LocalRedirect(returnUrl);
        }

        if (result.IsNotAllowed || result.IsLockedOut || !result.Succeeded)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "";
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "";

            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Error: Email information not provided by external provider.";
                return RedirectToAction("Login");
            }

            var user = new UserEntity
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (createResult.Succeeded)
            {
                bool isFirstUser = !await _dbInitService.IsDatabaseInitializedAsync();
                if (isFirstUser)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                    _logger.LogInformation("First user created with Admin role via external provider: {Email}", email);
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }

                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (addLoginResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                    return LocalRedirect(returnUrl);
                }
            }

            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return RedirectToAction("Login");
        }
        return RedirectToAction("Login");
    }


}
