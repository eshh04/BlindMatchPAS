// ============================================================================
// File: ViewModels/SupervisorDashboardViewModel.cs
// Purpose: View model for the supervisor dashboard, grouping matches by status.
// Reference: PUSL2020 Coursework - Section 4.2 (Supervisor Dashboard)
// ============================================================================

using BlindMatchPAS.Models;

namespace BlindMatchPAS.ViewModels;

/// <summary>
/// View model for the Supervisor/Dashboard view.
/// Groups the supervisor's match records into three categories by MatchStatus.
/// </summary>
public class SupervisorDashboardViewModel
{
    /// <summary>Matches where the supervisor has expressed interest but not yet confirmed.</summary>
    public List<Match> InterestedMatches { get; set; } = new();

    /// <summary>Matches that have been confirmed (identities revealed).</summary>
    public List<Match> ConfirmedMatches { get; set; } = new();

    /// <summary>Matches that were rejected (project matched to another supervisor).</summary>
    public List<Match> RejectedMatches { get; set; } = new();
}
