// ============================================================================
// File: DTOs/RevealedMatchDto.cs
// Purpose: Data Transfer Object for revealed match information passed to views.
// Pattern: DTO (Data Transfer Object) — decouples domain models from views.
// Reference: PUSL2020 Coursework - Section 4.1/4.2 (Identity Reveal)
// ============================================================================

namespace BlindMatchPAS.DTOs;

/// <summary>
/// DTO used to transfer revealed match data to the Reveal and SupervisorReveal views.
/// Contains the counterpart's name, email, and context-specific extra information.
/// PersonFullName/PersonEmail are generic — they represent the supervisor (for students)
/// or the student (for supervisors) depending on the calling context.
/// </summary>
public class RevealedMatchDto
{
    /// <summary>Match record ID.</summary>
    public int MatchId { get; set; }

    /// <summary>Full name of the revealed counterpart (supervisor or student).</summary>
    public string PersonFullName { get; set; } = string.Empty;

    /// <summary>Email address of the revealed counterpart.</summary>
    public string PersonEmail { get; set; } = string.Empty;

    /// <summary>Additional context (research areas for supervisor, project title for student).</summary>
    public string ExtraInfo { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the match was confirmed.</summary>
    public DateTime MatchedAt { get; set; }
}
