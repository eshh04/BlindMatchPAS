// ============================================================================
// File: ViewModels/MatchStatusViewModel.cs
// Purpose: View model for displaying a student's project match status.
// Reference: PUSL2020 Coursework - Section 4.1 (Student Match Tracking)
// ============================================================================

using BlindMatchPAS.Models;

namespace BlindMatchPAS.ViewModels;

/// <summary>
/// View model used by Match/Status view to display the current match
/// status of a student's project, including the status pipeline UI.
/// </summary>
public class MatchStatusViewModel
{
    /// <summary>The student's project entity (includes Status for pipeline display).</summary>
    public Project? Project { get; set; }

    /// <summary>The confirmed match record (null if not yet matched).</summary>
    public Match? Match { get; set; }

    /// <summary>Whether the project has a confirmed match.</summary>
    public bool IsMatched { get; set; }

    /// <summary>Whether the supervisor identity has been revealed to the student.</summary>
    public bool IsSupervisorRevealed { get; set; }
}
