// ============================================================================
// File: Controllers/AccountController.cs
// Purpose: Handles user authentication — login, registration, and logout.
//          Supports three roles: Student, Supervisor, Admin.
// Pattern: MVC Controller with ASP.NET Core Identity integration.
// Reference: PUSL2020 Coursework - Section 3 (Authentication & RBAC)
// ============================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BlindMatchPAS.Models;
using BlindMatchPAS.ViewModels;

namespace BlindMatchPAS.Controllers;

/// <summary>
/// Controller for user authentication operations.
/// AllowAnonymous permits access to login and register without authentication.
/// </summary>
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// GET: /Account/Login
    /// Displays the login form. Redirects authenticated users to their role-specific dashboard.
    /// </summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Redirect already-authenticated users to their dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Student"))
                return RedirectToAction("Dashboard", "Student");
            if (User.IsInRole("Supervisor"))
                return RedirectToAction("Dashboard", "Supervisor");
            if (User.IsInRole("Admin"))
                return RedirectToAction("RunMatching", "Match");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    /// POST: /Account/Login
    /// Validates credentials and signs in the user. Handles lockout on repeated failures.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            // Attempt sign-in with lockout enabled for brute-force protection
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in successfully");

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "User not found");
                    return View(model);
                }

                // Role-based redirect after successful login
                if (user.Role == "Admin")
                    return RedirectToAction("RunMatching", "Match");
                if (user.Role == "Student")
                    return RedirectToAction("Dashboard", "Student");
                if (user.Role == "Supervisor")
                    return RedirectToAction("Dashboard", "Supervisor");

                return LocalRedirect(returnUrl ?? "/");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out");
                ModelState.AddModelError(string.Empty,
                    "Account locked. Please try again later after 15 minutes.");
                return View(model);
            }

            _logger.LogWarning("Login attempt failed");
            ModelState.AddModelError(string.Empty, "Invalid email or password");
        }

        return View(model);
    }

    /// <summary>
    /// GET: /Account/Register
    /// Displays the registration form for new Student and Supervisor accounts.
    /// </summary>
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View();
    }

    /// <summary>
    /// POST: /Account/Register
    /// Creates a new user account, assigns role, and signs in automatically.
    /// Admin registration is blocked — only Student and Supervisor roles are permitted.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Security: Block Admin role registration (Admin is seeded only)
            if (string.IsNullOrEmpty(model.Role) ||
                (model.Role != "Student" && model.Role != "Supervisor"))
            {
                ModelState.AddModelError(nameof(model.Role), "Invalid role selection");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Role = model.Role
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with role {Role}", model.Role);

                // Assign Identity role for authorization policies
                await _userManager.AddToRoleAsync(user, model.Role);

                // Auto sign-in after registration
                await _signInManager.SignInAsync(user, isPersistent: false);

                if (model.Role == "Student")
                    return RedirectToAction("Dashboard", "Student");
                if (model.Role == "Supervisor")
                    return RedirectToAction("Dashboard", "Supervisor");

                return RedirectToAction("Index", "Home");
            }

            // Display Identity validation errors (e.g., duplicate email)
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    /// <summary>
    /// POST: /Account/Logout
    /// Signs out the current user and redirects to the login page.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out");
        return RedirectToAction("Login");
    }

    /// <summary>
    /// GET: /Account/AccessDenied
    /// Displayed when a user attempts to access a restricted resource.
    /// </summary>
    public IActionResult AccessDenied()
    {
        return View();
    }
}
