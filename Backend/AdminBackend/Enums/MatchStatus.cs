// ============================================================================
// File: Enums/MatchStatus.cs
// Purpose: Defines the states of a match between a supervisor and a project.
// Reference: PUSL2020 Coursework - Section 4.2 (Match Lifecycle)
// ============================================================================

namespace AdminBackend.Enums;

/// <summary>
/// Represents the status of a match record.
/// Transitions: Interested → Confirmed (or Interested → Rejected by algorithm).
/// </summary>
public enum MatchStatus
{
    /// <summary>Supervisor has expressed initial interest in the project.</summary>
    Interested = 0,

    /// <summary>Match confirmed; identities revealed and locked.</summary>
    Confirmed = 1,

    /// <summary>Interest rejected (project matched to another supervisor).</summary>
    Rejected = 2
}
