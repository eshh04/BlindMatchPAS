// ============================================================================
// File: ViewModels/RegisterViewModel.cs
// Purpose: View model for the user registration form with comprehensive validation.
// Pattern: ViewModel with Data Annotations, Regular Expressions, and [Compare].
// Reference: PUSL2020 Coursework - Section 3 (User Registration)
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.ViewModels;

/// <summary>
/// View model for the Account/Register form.
/// Validates name format, email format, password strength, and role selection.
/// </summary>
public class RegisterViewModel
{
    /// <summary>User full name — letters, spaces, hyphens, and apostrophes only.</summary>
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
    [RegularExpression(@"^[A-Za-z\s\-']+$",
        ErrorMessage = "Full name can only contain letters, spaces, hyphens, and apostrophes")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>User email — validated with regex for standard email format.</summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "Please enter a valid email address (e.g., user@university.edu)")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Password — minimum 8 characters with strength requirements enforced by Identity.</summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
        ErrorMessage = "Password must contain uppercase, lowercase, digit, and special character")]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Password confirmation — must match the Password field.</summary>
    [Required(ErrorMessage = "Please confirm your password")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>Selected role: "Student" or "Supervisor" (Admin registration is blocked).</summary>
    [Required(ErrorMessage = "Please select a role")]
    [RegularExpression(@"^(Student|Supervisor)$", ErrorMessage = "Invalid role selection")]
    [Display(Name = "I am a...")]
    public string Role { get; set; } = string.Empty;
}
