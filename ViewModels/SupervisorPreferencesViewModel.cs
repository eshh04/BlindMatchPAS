// ============================================================================
// File: ViewModels/SupervisorPreferencesViewModel.cs
// Purpose: View model for the supervisor research area preferences management page.
// Reference: PUSL2020 Coursework - Section 4.2 (Supervisor Preference Ranking)
// ============================================================================

using BlindMatchPAS.Models;

namespace BlindMatchPAS.ViewModels;

/// <summary>
/// View model for the Supervisor/Preferences view.
/// Contains all research areas, current preferences, and areas not yet selected.
/// </summary>
public class SupervisorPreferencesViewModel
{
    /// <summary>Complete list of all research areas in the system.</summary>
    public List<ResearchArea> AllResearchAreas { get; set; } = new();

    /// <summary>Supervisor's current ranked preferences (ordered by PreferenceRank).</summary>
    public List<SupervisorPreference> Preferences { get; set; } = new();

    /// <summary>Research areas not yet added to the supervisor's preferences.</summary>
    public List<ResearchArea> AvailableAreas { get; set; } = new();
}
