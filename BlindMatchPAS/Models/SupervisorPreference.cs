// ============================================================================
// File: Models/SupervisorPreference.cs
// Purpose: Entity model representing a supervisor's ranked preference for
//          a research area. Used by the Gale-Shapley stable matching algorithm.
// Pattern: Code-First EF Core entity with unique constraint on (SupervisorId, ResearchAreaId).
// Reference: PUSL2020 Coursework - Section 4.2 (Supervisor Research Area Preferences)
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models;

/// <summary>
/// Represents a supervisor's preference ranking for a specific research area.
/// Lower PreferenceRank values indicate higher priority in the matching algorithm.
/// Constrained to one preference per supervisor per research area (unique index).
/// </summary>
public class SupervisorPreference
{
    /// <summary>Primary key for the preference entity.</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>Foreign key referencing the supervisor (ApplicationUser) who set this preference.</summary>
    [Required]
    public string SupervisorId { get; set; } = string.Empty;

    /// <summary>Navigation property to the supervisor user.</summary>
    public ApplicationUser? Supervisor { get; set; }

    /// <summary>Foreign key referencing the research area this preference applies to.</summary>
    [Required]
    public int ResearchAreaId { get; set; }

    /// <summary>Navigation property to the research area.</summary>
    public ResearchArea? ResearchArea { get; set; }

    /// <summary>
    /// Preference rank: 1 = most preferred, 2 = second, etc.
    /// The Gale-Shapley algorithm uses this to order supervisor proposals.
    /// </summary>
    [Required]
    public int PreferenceRank { get; set; }

    /// <summary>UTC timestamp when this preference was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
