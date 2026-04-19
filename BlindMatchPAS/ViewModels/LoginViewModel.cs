// ============================================================================
// File: ViewModels/LoginViewModel.cs
// Purpose: View model for the user login form with email and password validation.
// Pattern: ViewModel with Data Annotations and Regular Expressions.
// Reference: PUSL2020 Coursework - Section 3 (Authentication)
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.ViewModels;

/// <summary>
/// View model for the Account/Login form.
/// Validates email format using both [EmailAddress] and [RegularExpression].
/// </summary>
public class LoginViewModel
{
    /// <summary>User email — validated with regex for standard email format.</summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "Please enter a valid email address (e.g., user@example.com)")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    /// <summary>User password (masked input).</summary>
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Whether to persist the authentication cookie beyond the session.</summary>
    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}
