// ============================================================================
// File: ViewModels/EditProjectViewModel.cs
// Purpose: View model for the project edit form with robust validation.
// Pattern: ViewModel with Data Annotations and Regular Expressions.
// Reference: PUSL2020 Coursework - Section 4.1 (Student Project Editing)
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.ViewModels;

/// <summary>
/// View model for the Student/Edit form. Mirrors CreateProjectViewModel
/// with an additional Id property for identifying the project to update.
/// </summary>
public class EditProjectViewModel
{
    /// <summary>Project ID (hidden field for form binding).</summary>
    public int Id { get; set; }

    /// <summary>Project title — must start with a letter (10-200 characters).</summary>
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "Title must be between 10 and 200 characters")]
    [RegularExpression(@"^[A-Za-z].*", ErrorMessage = "Title must start with a letter")]
    [Display(Name = "Project Title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Project abstract — must start with a letter or number (50-2000 characters).</summary>
    [Required(ErrorMessage = "Abstract is required")]
    [StringLength(2000, MinimumLength = 50, ErrorMessage = "Abstract must be between 50 and 2000 characters")]
    [RegularExpression(@"^[A-Za-z0-9].*", ErrorMessage = "Abstract must start with a letter or number")]
    [Display(Name = "Project Abstract")]
    public string Abstract { get; set; } = string.Empty;

    /// <summary>Comma-separated technology stack (e.g., "Python, React, SQL").</summary>
    [Required(ErrorMessage = "Tech stack is required")]
    [StringLength(500, ErrorMessage = "Tech Stack cannot exceed 500 characters")]
    [RegularExpression(@"^[A-Za-z0-9#\+\.\s]+([,;]\s*[A-Za-z0-9#\+\.\s]+)*$",
        ErrorMessage = "Tech stack must be a comma-separated list (e.g., Python, React, SQL)")]
    [Display(Name = "Technology Stack")]
    public string TechStack { get; set; } = string.Empty;

    /// <summary>Selected research area ID from the dropdown.</summary>
    [Required(ErrorMessage = "Please select a research area")]
    [Display(Name = "Research Area")]
    public int ResearchAreaId { get; set; }
}
