// ============================================================================
// File: Models/ApplicationUser.cs
// Purpose: Extended ASP.NET Core Identity user model for the BlindMatch system.
//          Adds FullName, Role, and navigation properties for domain relationships.
// Pattern: Identity extension with Data Annotation validation.
// Reference: PUSL2020 Coursework - Section 3 (Authentication & RBAC)
// ============================================================================

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models;

/// <summary>
/// Extended Identity user model supporting three roles: Student, Supervisor, and Admin.
/// Includes navigation properties to projects, matches, and research area preferences.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Full name of the user. Must contain only letters and spaces (2-100 characters).
    /// Regex enforces alphabetic names (e.g., "John Smith").
    /// </summary>
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
    [RegularExpression(@"^[A-Za-z\s\-']+$", ErrorMessage = "Full name can only contain letters, spaces, hyphens, and apostrophes")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Role assignment for this user: "Student", "Supervisor", or "Admin".
    /// Stored redundantly alongside Identity roles for quick access in queries.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Collection of projects submitted by this user (applicable when Role == "Student").</summary>
    public ICollection<Project>? StudentProjects { get; set; }

    /// <summary>Collection of match records where this user is the supervisor.</summary>
    public ICollection<Match>? SupervisorMatches { get; set; }

    /// <summary>Ranked research area preferences for this supervisor (used by Gale-Shapley algorithm).</summary>
    public ICollection<SupervisorPreference>? ResearchAreaPreferences { get; set; }
}
