// ============================================================================
// File: Models/Match.cs
// Purpose: Entity model representing a match between a supervisor and a
//          student project proposal in the blind matching system.
// Pattern: Code-First EF Core entity with Data Annotation validation.
// Reference: PUSL2020 Coursework - Section 4.2 (Supervisor Functionality)
// ============================================================================

using System.ComponentModel.DataAnnotations;
using BlindMatchPAS.Enums;

namespace BlindMatchPAS.Models;

/// <summary>
/// Represents a match between a supervisor and a project proposal.
/// Tracks the lifecycle: Interest Expression → Confirmation → Identity Reveal.
/// Multiple supervisors can express interest in the same project (many-to-one with Project).
/// </summary>
public class Match
{
    /// <summary>Primary key for the match entity.</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>Foreign key referencing the project being matched.</summary>
    [Required]
    public int ProjectId { get; set; }

    /// <summary>Navigation property to the associated project.</summary>
    public Project? Project { get; set; }

    /// <summary>Foreign key referencing the supervisor (ApplicationUser) interested in this project.</summary>
    [Required]
    public string SupervisorId { get; set; } = string.Empty;

    /// <summary>Navigation property to the supervisor user.</summary>
    public ApplicationUser? Supervisor { get; set; }

    /// <summary>
    /// Current status of this match record (Interested → Confirmed / Rejected).
    /// Managed by MatchingService during interest expression, confirmation, and Gale-Shapley execution.
    /// </summary>
    public MatchStatus Status { get; set; } = MatchStatus.Interested;

    /// <summary>UTC timestamp when this match record was initially created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp when the match was confirmed (null if not yet confirmed).</summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// Whether identities have been revealed to both parties.
    /// Set to true only after match confirmation — enforces blind matching privacy.
    /// </summary>
    public bool IsRevealed { get; set; } = false;
}
