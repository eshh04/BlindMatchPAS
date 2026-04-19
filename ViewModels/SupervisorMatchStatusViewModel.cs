// ============================================================================
// File: ViewModels/SupervisorMatchStatusViewModel.cs
// Purpose: View model for displaying match status from the supervisor's perspective.
// Reference: PUSL2020 Coursework - Section 4.2 (Supervisor Match Tracking)
// ============================================================================

using BlindMatchPAS.Models;

namespace BlindMatchPAS.ViewModels;

/// <summary>
/// View model for the Match/SupervisorStatus view.
/// Displays match details and confirmation/reveal state from the supervisor's perspective.
/// </summary>
public class SupervisorMatchStatusViewModel
{
    /// <summary>The match record including related project and student data.</summary>
    public Match? Match { get; set; }

    /// <summary>Whether the match has been confirmed by the supervisor.</summary>
    public bool IsConfirmed { get; set; }

    /// <summary>Whether identities have been revealed.</summary>
    public bool IsRevealed { get; set; }

    /// <summary>Display name of the matched student (shown after confirmation).</summary>
    public string StudentName { get; set; } = string.Empty;
}
