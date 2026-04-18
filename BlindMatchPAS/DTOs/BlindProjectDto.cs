// ============================================================================
// File: DTOs/BlindProjectDto.cs
// Purpose: Data Transfer Object for project data with student identity removed.
// Pattern: DTO — intentionally omits StudentId/StudentName/StudentEmail
//          to enforce blind matching privacy requirements.
// Reference: PUSL2020 Coursework - Section 2 (Blind Matching Concept)
// ============================================================================

namespace BlindMatchPAS.DTOs;

/// <summary>
/// Blind project DTO that supervisors see when browsing projects.
/// Student identity fields are intentionally omitted to preserve anonymity
/// until a match is confirmed and identities are revealed.
/// </summary>
public class BlindProjectDto
{
    /// <summary>Project ID.</summary>
    public int Id { get; set; }

    /// <summary>Project title (visible to supervisors).</summary>
    public string? Title { get; set; }

    /// <summary>Project abstract (visible to supervisors).</summary>
    public string? Abstract { get; set; }

    /// <summary>Technology stack (visible to supervisors).</summary>
    public string? TechStack { get; set; }

    /// <summary>Research area name for display.</summary>
    public string? ResearchAreaName { get; set; }

    /// <summary>Research area navigation (for grouping/filtering).</summary>
    public object? ResearchArea { get; set; }

    /// <summary>Submission timestamp.</summary>
    public DateTime SubmittedAt { get; set; }

    /// <summary>Current project status as integer.</summary>
    public int Status { get; set; }

    // INTENTIONALLY OMITTED: StudentId, StudentName, StudentEmail
    // This enforces the blind matching privacy requirement.
}
