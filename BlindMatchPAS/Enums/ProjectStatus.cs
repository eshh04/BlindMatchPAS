// ============================================================================
// File: Enums/ProjectStatus.cs
// Purpose: Defines the lifecycle states of a student project proposal.
// Reference: PUSL2020 Coursework - Section 4.1 (Project Lifecycle)
// ============================================================================

namespace BlindMatchPAS.Enums;

/// <summary>
/// Represents the lifecycle status of a student project proposal.
/// Transitions: Pending → UnderReview → Matched (or Pending/UnderReview → Withdrawn).
/// </summary>
public enum ProjectStatus
{
    /// <summary>Project submitted, awaiting supervisor interest.</summary>
    Pending = 0,

    /// <summary>At least one supervisor has expressed interest; under active review.</summary>
    UnderReview = 1,

    /// <summary>Project matched to a supervisor via the stable matching algorithm.</summary>
    Matched = 2,

    /// <summary>Project withdrawn by the student before matching.</summary>
    Withdrawn = 3
}
