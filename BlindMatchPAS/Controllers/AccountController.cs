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
        // Redirect already-authenticated users to their dashboard, UNLESS we're showing a security error
        if (User.Identity?.IsAuthenticated == true && string.IsNullOrEmpty(returnUrl) && Request.Query["error"] != "access_denied")
        {
            if (User.IsInRole("Student"))
                return RedirectToAction("Dashboard", "Student");
            if (User.IsInRole("Supervisor"))
                return RedirectToAction("Dashboard", "Supervisor");
            if (User.IsInRole("Admin"))
                return RedirectToAction("Dashboard", "Admin");
        }

        if (Request.Query["error"] == "access_denied")
        {
            ModelState.AddModelError(string.Empty, "Security Alert: You do not have permission to access that section. Please log in with an authorized account or contact the system administrator.");
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
                var roles = await _userManager.GetRolesAsync(user);
                
                if (user.Role == "Admin" || roles.Contains("Admin"))
                    return RedirectToAction("Dashboard", "Admin");
                if (user.Role == "Student" || roles.Contains("Student"))
                    return RedirectToAction("Dashboard", "Student");
                if (user.Role == "Supervisor" || roles.Contains("Supervisor"))
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
            // Security: Public registration is restricted to Students ONLY.
            // Supervisors and Admins must be registered via the Admin User Management portal.
            if (model.Role != "Student")
            {
                _logger.LogWarning("Security Breach Attempt: Public registration attempted for unauthorized role: {Role}", model.Role);
                ModelState.AddModelError(nameof(model.Role), "Only student registration is permitted through this portal. Please contact your administrator for other account types.");
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
        return RedirectToAction("Login", new { error = "access_denied" });
    }

    /// <summary>
    /// GET: /Account/ForgotPassword
    /// Displays the forgot password form where the user enters their email.
    /// </summary>
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    /// <summary>
    /// POST: /Account/ForgotPassword
    /// Verifies the email exists and redirects to the reset password page.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(string.Empty, "Please enter your email address.");
            return View();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "No account found with that email address.");
            return View();
        }

        // Redirect directly to the reset form — no email token required (local system)
        return RedirectToAction("ResetPassword", new { email = email });
    }

    /// <summary>
    /// GET: /Account/ResetPassword
    /// Displays the password reset form for the verified email.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ResetPassword(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return RedirectToAction("ForgotPassword");

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return RedirectToAction("ForgotPassword");

        ViewBag.Email = email;
        ViewBag.FullName = user.FullName;
        return View();
    }

    /// <summary>
    /// POST: /Account/ResetPassword
    /// Directly resets the user password using Identity's token-based reset.
    /// Works locally without email — generates and immediately consumes the token.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string email, string newPassword, string confirmPassword)
    {
        ViewBag.Email = email;

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
        {
            ModelState.AddModelError(string.Empty, "Password must be at least 8 characters long.");
            return View();
        }

        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Passwords do not match.");
            return View();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Account not found.");
            return View();
        }

        // Generate a reset token and immediately use it (local system — no email needed)
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            _logger.LogInformation("Password reset successfully for user {Email}", email);
            TempData["SuccessMessage"] = "Password reset successfully! You can now log in with your new password.";
            return RedirectToAction("Login");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View();
    }
}
