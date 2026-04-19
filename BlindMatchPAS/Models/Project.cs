// ============================================================================
// File: Models/Project.cs
// Purpose: Entity model representing a student's project proposal submission
//          within the BlindMatch blind-matching system.
// Pattern: Code-First EF Core entity with Data Annotation validation.
// Reference: PUSL2020 Coursework - Section 4.1 (Student Functionality)
// ============================================================================

using System.ComponentModel.DataAnnotations;
using BlindMatchPAS.Enums;

namespace BlindMatchPAS.Models;

/// <summary>
/// Represents a project proposal submitted by a student in the blind matching system.
/// Each student can submit multiple projects; each project can receive interest
/// from multiple supervisors (one-to-many with Match).
/// </summary>
public class Project
{
    /// <summary>Primary key for the project entity.</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>Foreign key referencing the student (ApplicationUser) who submitted this project.</summary>
    [Required]
    public string StudentId { get; set; } = string.Empty;

    /// <summary>Navigation property to the student user who owns this project.</summary>
    public ApplicationUser? Student { get; set; }

    /// <summary>
    /// Project title — must start with a letter and be 10-200 characters.
    /// Regex ensures the title begins with an alphabetic character.
    /// </summary>
    [Required(ErrorMessage = "Project title is required")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "Title must be between 10 and 200 characters")]
    [RegularExpression(@"^[A-Za-z].*", ErrorMessage = "Title must start with a letter")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed project abstract describing goals and technical approach (50-2000 characters).
    /// Regex ensures the abstract starts with an alphanumeric character.
    /// </summary>
    [Required(ErrorMessage = "Abstract is required")]
    [StringLength(2000, MinimumLength = 50, ErrorMessage = "Abstract must be between 50 and 2000 characters")]
    [RegularExpression(@"^[A-Za-z0-9].*", ErrorMessage = "Abstract must start with a letter or number")]
    public string Abstract { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of technologies and tools required for this project.
    /// Regex validates a comma-separated format (e.g., "Python, Flask, PostgreSQL").
    /// </summary>
    [Required(ErrorMessage = "Tech stack is required")]
    [StringLength(500, ErrorMessage = "Tech stack cannot exceed 500 characters")]
    [RegularExpression(@"^[A-Za-z0-9#\+\.\s]+([,;]\s*[A-Za-z0-9#\+\.\s]+)*$",
        ErrorMessage = "Tech stack must be a comma-separated list of technologies (e.g., Python, React, SQL)")]
    public string TechStack { get; set; } = string.Empty;

    /// <summary>Foreign key referencing the research area classification for this project.</summary>
    [Required]
    public int ResearchAreaId { get; set; }

    /// <summary>Navigation property to the research area this project belongs to.</summary>
    public ResearchArea? ResearchArea { get; set; }

    /// <summary>
    /// Current lifecycle status of this project (Pending → UnderReview → Matched / Withdrawn).
    /// Managed by the ProjectService and MatchingService.
    /// </summary>
    public ProjectStatus Status { get; set; } = ProjectStatus.Pending;

    /// <summary>UTC timestamp when the project was initially submitted.</summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of the most recent update to this project (nullable).</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Whether the student and supervisor identities have been revealed for this project.
    /// Handled by the Admin to control the blind-matching lifecycle.
    /// </summary>
    public bool IsRevealed { get; set; } = false;

    /// <summary>Whether the project was submitted after the deadline.</summary>
    public bool IsLate { get; set; } = false;

    /// <summary>Indicates if this is a group project (multiple students).</summary>
    public bool IsGroupProject { get; set; } = false;

    /// <summary>Comma-separated list of group member emails (excluding the submitter).</summary>
    public string? GroupMemberEmails { get; set; }

    /// <summary>
    /// Collection of match records associated with this project.
    /// A project can receive interest from multiple supervisors (one-to-many relationship).
    /// </summary>
    public ICollection<Match>? Matches { get; set; }
}
